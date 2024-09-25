using Hermes.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(HermesContext db) : BaseRepository<UnitUnderTest>(db)
{
    public async Task<List<UnitUnderTest>> GetLastUnitsUnderTest(int qty)
    {
        // Entity framework + Linq
        return await Db
            .UnitsUnderTest
            .OrderByDescending(x => x.Id)
            .Take(qty)
            .ToListAsync();
    }

    public async Task<List<UnitUnderTest>> Find(string sn, string msg)
    {
        return await Db
            .UnitsUnderTest
            .Where(x => x.SerialNumber == sn && x.Content == msg)
            .ToListAsync();
    }

    public async Task AddExampleDataAsync()
    {
        //crear ejemplos de Stop y SfcResponse
        var stop0 = new Stop { Type = (Types.StopType)0, IsRestored = false };
        var stop2 = new Stop { Type = (Types.StopType)2, IsRestored = true };

        var sfcResponse0 = new SfcResponse { ResponseType = (Types.SfcResponseType)0, Content = "Response content 1" };
        var sfcResponse1 = new SfcResponse { ResponseType = (Types.SfcResponseType)1, Content = "Response content 1" };
        var sfcResponse2 = new SfcResponse { ResponseType = (Types.SfcResponseType)2, Content = "Response content 2" };

        db.Stops.AddRange(stop0, stop2);
        db.SfcResponses.AddRange(sfcResponse0,sfcResponse1, sfcResponse2);

        await db.SaveChangesAsync();

        var example0 = new UnitUnderTest
        {
            FileName = "1A62LRA00303BP1SS.3dx",
            SerialNumber = "1A62LRA00303BP1SS",
            IsFail = false,
            CreatedAt = DateTime.Now,
            Stop = stop0,
            SfcResponse = sfcResponse0
        };

        var example1 = new UnitUnderTest
        {
            FileName = "1A62LRA00303BP10M.3dx",
            SerialNumber = "1A62LRA00303BP10M",
            IsFail = false,
            CreatedAt = DateTime.Now,
            Stop = stop0,
            SfcResponse = sfcResponse1
        };

        var example2 = new UnitUnderTest
        {
            FileName = "example2.3dx",
            SerialNumber = "SN987654321",
            IsFail = true,
            CreatedAt = DateTime.Now,
            Stop = stop2,
            SfcResponse = sfcResponse2
        };

        db.Set<UnitUnderTest>().AddRange(example0, example1, example2);

        await db.SaveChangesAsync();
    }

    /*
    public List<UnitUnderTest> GetAllUnits()
    {
        return GetAll();
    }
    */
    public async Task<List<UnitUnderTest>> GetAllUnits()
    {
        return await db.Set<UnitUnderTest>().ToListAsync();
    }


    /*
    public async Task<UnitUnderTest?> FindBySerialNumberAsync(string serialNumber)
    {
        return await db.Set<UnitUnderTest>()
            .FirstOrDefaultAsync(u => u.SerialNumber == serialNumber);
    }
    */

    public async Task<List<UnitUnderTest>> FindBySerialNumberAsync(string serialNumber)
    {
        return await Db.Set<UnitUnderTest>()
            .Where(u => u.SerialNumber.Contains(serialNumber.ToUpper()))
            .ToListAsync();
    }

}