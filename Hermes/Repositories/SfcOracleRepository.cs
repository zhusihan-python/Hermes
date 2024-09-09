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
    private const string ConString = "Data Source=10.12.171.61:1526/GDLSFCDB;User Id=MPROGRAM;Password=GOODDFMS;";

    public async Task<IEnumerable<UnitUnderTest>> GetAllUnitsUnderTest(string pkgid)
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

    public async Task<Package> GetPackage(string pkgid)
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

    public async Task<WorkOrder> GetWorkOrder(string workOrder)
    {
        var result = await this.Query<WorkOrder>($"""
                                                  SELECT 
                                                      WORK_ORDER AS Id,
                                                      PART_NO As PartNumber,
                                                      PART_VERSION AS Revision
                                                  FROM SFISM4.WIP_D_WO_MASTER
                                                  WHERE WORK_ORDER = :workOrder
                                                  """, new { workOrder });
        return result.FirstOrDefault(WorkOrder.Null);
    }

    public async Task<IEnumerable<Package>> GetAllPackages(string workOrder)
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