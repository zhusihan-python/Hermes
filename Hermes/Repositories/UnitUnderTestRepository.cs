using System;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(HermesLocalContext db) : BaseRepository<UnitUnderTest, HermesLocalContext>(db)
{
    private readonly HermesLocalContext _db = db;

    public async Task<List<UnitUnderTest>> GetLastUnitsUnderTest(int qty)
    {
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

    public async Task<List<UnitUnderTest>> GetAllLast24HrsUnits()
    {
        return await _db.Set<UnitUnderTest>()
            .Include(x => x.SfcResponse)
            .Where(x => x.CreatedAt >= DateTime.Now.AddDays(-1))
            .ToListAsync();
    }

    public async Task<List<UnitUnderTest>> FindBySerialNumberAsync(string serialNumber)
    {
        return await Db.Set<UnitUnderTest>()
            .Where(u => u.SerialNumber.Contains(serialNumber.ToUpper()))
            .ToListAsync();
    }
}