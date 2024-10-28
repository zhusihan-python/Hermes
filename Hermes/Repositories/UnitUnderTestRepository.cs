using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<UnitUnderTest, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task<List<UnitUnderTest>> GetLastUnitsUnderTest(int qty)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx
            .UnitsUnderTest
            .OrderByDescending(x => x.Id)
            .Take(qty)
            .ToListAsync();
    }

    public async Task<List<UnitUnderTest>> Find(string sn, string msg)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx
            .UnitsUnderTest
            .Where(x => x.SerialNumber == sn && x.Content == msg)
            .ToListAsync();
    }

    public async Task<List<UnitUnderTest>> GetFromLast24HrsUnits(
        string? serialNumber = null,
        StatusType? statusType = null,
        SfcResponseType? sfcResponseType = null)
    {
        var ctx = await _context.CreateDbContextAsync();
        var query = GetAllLast24HrsUnitsQuery(ctx);
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
        await using var ctx = await _context.CreateDbContextAsync();
        return await GetAllLast24HrsUnitsQuery(ctx)
            .ToListAsync();
    }

    private IQueryable<UnitUnderTest> GetAllLast24HrsUnitsQuery(HermesLocalContext ctx)
    {
        return ctx.Set<UnitUnderTest>()
            .Include(x => x.SfcResponse)
            .Where(x => x.CreatedAt >= DateTime.Now.AddDays(-1));
    }

    public async Task<List<UnitUnderTest>> FindBySerialNumberAsync(string serialNumber)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.Set<UnitUnderTest>()
            .Where(u => u.SerialNumber.Contains(serialNumber.ToUpper()))
            .ToListAsync();
    }
}