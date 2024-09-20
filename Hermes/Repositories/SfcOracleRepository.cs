using Dapper;
using Hermes.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public class SfcOracleRepository
{
    private const string ConString = "Data Source=10.12.171.61:1526/GDLSFCDB;User Id=SFIS1;Password=sfis1;";

    public async Task<int> UpdatePackageTrackingLoadedAt(string pkgid)
    {
        return await this.ExecuteQueryAsync($"""
                                             UPDATE SFISM4.H_PACKAGES_TRACK
                                             SET LOADED_AT = SYSTIMESTAMP
                                             WHERE PKGID = :pkgid
                                             """, new { pkgid });
    }

    public async Task<Package> FindPackageTracking(string pkgid)
    {
        return (await this.FindAllPackagesTrackingByPkgid(pkgid)).FirstOrDefault(Package.Null);
    }

    public async Task<IEnumerable<Package>> FindAllPackagesTrackingByPkgid(string pkgid)
    {
        return await this.FindAllPackagesTracking(pkgid: pkgid);
    }

    public async Task<IEnumerable<Package>> FindAllPackagesTrackingByDate(string line, DateTime fromDate,
        DateTime toDate)
    {
        return await this.FindAllPackagesTracking(line, fromDate, toDate);
    }

    private async Task<IEnumerable<Package>> FindAllPackagesTracking(
        string? line = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? pkgid = null)
    {
        var whereClause = "pkg_track.PKGID = :pkgid";
        if (string.IsNullOrEmpty(pkgid))
        {
            whereClause = "pkg_track.LINE = :line AND pkg_track.SCANNED_AT BETWEEN :fromDate AND :toDate";
        }

        return await this.Query<Package>($"""
                                          SELECT MAX(pkg_track.PKGID)             AS Id,
                                                 MAX(pkg_track.LINE)              AS Line,
                                                 MAX(SFIS1.C_PCB_PRINT_T.QTY)     AS Quantity,
                                                 COUNT(*)                         AS QuantityUsed,
                                                 MAX(CDATE)                       AS OpenedAt,
                                                 MAX(pkg_track.LOADED_AT)         AS LoadedAt,
                                                 MAX(SFIS1.C_PCB_PRINT_T.IN_TIME) AS LastUsedAt,
                                                 MAX(pkg_track.SCANNED_AT)        AS ScannedAt
                                          FROM SFISM4.H_PACKAGES_TRACK pkg_track
                                                   LEFT JOIN SFISM4.R_PKGID_BOM_T ON pkg_track.PKGID = R_PKGID_BOM_T.PKG_ID
                                                   LEFT JOIN SFIS1.C_PCB_PRINT_T ON pkg_track.PKGID = SFIS1.C_PCB_PRINT_T.PKGID
                                          WHERE 
                                              {whereClause}
                                          GROUP BY pkg_track.PKGID
                                          ORDER BY ScannedAt DESC, OpenedAt DESC
                                          """, new { line, fromDate, toDate, pkgid });
    }

    public async Task<IEnumerable<UnitUnderTest>> FindAllUnitsUnderTest(string pkgid)
    {
        return await this.Query<UnitUnderTest>($"""
                                                SELECT
                                                    rownum AS Id,
                                                    SerialNumber,
                                                    CreatedAt
                                                FROM (
                                                    SELECT 
                                                        SERIAL_NUMBER AS SerialNumber,  
                                                        IN_TIME AS CreatedAt 
                                                    FROM SFIS1.C_PCB_PRINT_T 
                                                    WHERE PKGID = :pkgid
                                                    ORDER BY IN_TIME)
                                                ORDER BY Id DESC
                                                """, new { pkgid });
    }

    public async Task<Package> FindPackage(string pkgid)
    {
        var result = await this.Query<Package>($"""
                                                SELECT PKG_ID      AS Id,
                                                       QTY         AS Quantity,
                                                       CDATE       AS Opened,
                                                       DATE_CODE   AS DateCode,
                                                       LOT_NO      AS Lot,
                                                       VENDOR_NAME AS Supply,
                                                       TIPTOP_ORD AS workOrder,
                                                       VENDOR_NAME AS Vendor
                                                FROM sfism4.R_PKGID_BOM_T
                                                WHERE PKG_ID = :pkgid
                                                """, new { pkgid });
        return result.FirstOrDefault(Package.Null);
    }

    public async Task<WorkOrder> FindWorkOrder(string workOrder)
    {
        var result = await this.Query<WorkOrder>($"""
                                                  SELECT
                                                      WORK_ORDER AS Id,
                                                      PART_NO As PartNumber,
                                                      PART_VERSION AS Revision,
                                                      MODEL_SERIAL As ModelName
                                                  FROM 
                                                      SFISM4.WIP_D_WO_MASTER
                                                      LEFT JOIN C_MODEL_DESC_T 
                                                          ON C_MODEL_DESC_T.MODEL_NAME = WIP_D_WO_MASTER.PART_NO AND 
                                                             C_MODEL_DESC_T.REV = WIP_D_WO_MASTER.PART_VERSION
                                                  WHERE WORK_ORDER = :workOrder
                                                  """, new { workOrder });
        return result.FirstOrDefault(WorkOrder.Null);
    }

    public async Task<IEnumerable<Package>> FindAllPackages(string workOrder)
    {
        return await this.Query<Package>($"""
                                          SELECT 
                                              PKGID         AS Id,
                                              MAX(QTY)       AS Quantity,
                                              COUNT(*)       As QuantityUsed,
                                              MIN(IN_TIME)   AS Opened,
                                              MAX(DATE_CODE) AS DateCode,
                                              MAX(LOT_CODE)  AS Lot,
                                              MAX(SUPPLY)    AS Vendor
                                          FROM SFIS1.C_PCB_PRINT_T
                                          WHERE MO_NO = :workOrder
                                          GROUP BY PKGID
                                          ORDER BY Opened DESC
                                          """, new { workOrder });
    }

    public async Task<int> AddPackageTrack(Package package)
    {
        return await this.ExecuteQueryAsync($"""
                                             INSERT INTO SFISM4.H_PACKAGES_TRACK
                                             VALUES(:pkgid, :line, NULL, SYSTIMESTAMP)

                                             """, new { pkgid = package.NormalizedId, line = package.Line });
    }

    private async Task<IEnumerable<T>> Query<T>(string sql, object? param = null)
    {
        await using var connection = new OracleConnection(ConString);
        try
        {
            connection.Open();
            return await connection.QueryAsync<T>(sql, param);
        }
        catch (Exception e) when (e is OracleException)
        {
            throw;
        }
        catch (Exception)
        {
            return new List<T>();
        }
    }

    private async Task<int> ExecuteQueryAsync(string sql, object? param = null)
    {
        await using var connection = new OracleConnection(ConString);
        try
        {
            connection.Open();
            return await connection.ExecuteAsync(sql, param);
        }
        catch (Exception e) when (e is OracleException)
        {
            throw;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public bool VerifyNextGroup(string serialNumber, string expectedGroup, out string nextGroup)
    {
        using OracleConnection con = new OracleConnection(ConString);
        using OracleCommand cmd = con.CreateCommand();
        try
        {
            con.Open();
            cmd.BindByName = true;

            cmd.CommandText = $"SFISM4.SP_VERIFY_ROUTE";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("serialNumber", OracleDbType.Varchar2).Value = serialNumber;
            cmd.Parameters.Add("expectedGroup", OracleDbType.Varchar2).Value = expectedGroup;
            cmd.Parameters.Add("route", OracleDbType.Varchar2, 250);
            cmd.Parameters["route"].Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            nextGroup = cmd.Parameters["route"].Value?.ToString() ?? string.Empty;
            if (nextGroup.Contains("OK", StringComparison.OrdinalIgnoreCase))
            {
                nextGroup = expectedGroup;
            }

            nextGroup = nextGroup.Replace("GO-", "");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            nextGroup = "";
        }

        return nextGroup.Contains(expectedGroup, StringComparison.OrdinalIgnoreCase);
    }
}