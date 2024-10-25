using System;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Types;

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

    public async Task<List<UnitUnderTest>> GetFromLast24HrsUnits(
        string? serialNumber = null,
        StatusType? statusType = null,
        SfcResponseType? sfcResponseType = null)
    {
        var query = GetAllLast24HrsUnitsQuery();
        if (serialNumber != null)
        {
            query = query.Where(x => x.SerialNumber
                .Contains(serialNumber, StringComparison.CurrentCultureIgnoreCase));
        }

        if (statusType != null)
        {
            query = query.Where(x => x.IsFail == statusType.IsFail());
        }

        if (sfcResponseType != null)
        {
            query = query.Where(x => x.SfcResponse != null && x.SfcResponse.Type == sfcResponseType);
        }

        return await query.ToListAsync();
    }

    public async Task<List<UnitUnderTest>> GetAllLast24HrsUnits()
    {
        return await GetAllLast24HrsUnitsQuery()
            .ToListAsync();
    }

    private IQueryable<UnitUnderTest> GetAllLast24HrsUnitsQuery()
    {
        return _db.Set<UnitUnderTest>()
            .Include(x => x.SfcResponse)
            .Where(x => x.CreatedAt >= DateTime.Now.AddDays(-1));
    }

    public async Task<List<UnitUnderTest>> FindBySerialNumberAsync(string serialNumber)
    {
        return await Db.Set<UnitUnderTest>()
            .Where(u => u.SerialNumber.Contains(serialNumber.ToUpper()))
            .ToListAsync();
    }
}