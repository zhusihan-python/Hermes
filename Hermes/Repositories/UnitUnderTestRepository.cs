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

    public async Task<List<UnitUnderTest>> GetLastUnits(
        string? serialNumber = null,
        StatusType? statusType = null,
        SfcResponseType? sfcResponseType = null,
        string? sfcResponseContent = null,
        TimeSpan? lastTimeSpan = null,
        int limit = 500)
    {
        if (!string.IsNullOrWhiteSpace(serialNumber) || !string.IsNullOrWhiteSpace(sfcResponseContent))
        {
            lastTimeSpan = null;
        }

        var ctx = await _context.CreateDbContextAsync();
        var query = GetAllUnitsFromLastTimeStanQuery(
            ctx,
            lastTimeSpan,
            limit);
        if (serialNumber != null)
        {
            query = query.Where(x => EF.Functions.Like(x.SerialNumber, $"%{serialNumber}%"));
        }

        if (statusType != null)
        {
            query = query.Where(x => x.IsFail == statusType.IsFail());
        }

        if (sfcResponseType != null)
        {
            query = query.Where(x => x.SfcResponse != null && x.SfcResponse.Type == sfcResponseType);
        }

        if (sfcResponseContent != null)
        {
            query = query.Where(x =>
                x.SfcResponse != null && EF.Functions.Like(x.SfcResponse.Content, $"%{sfcResponseContent}%"));
        }

        return await query.ToListAsync();
    }

    public async Task<List<UnitUnderTest>> GetAllLast24HrsUnits()
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await GetAllUnitsFromLastTimeStanQuery(ctx, TimeSpan.FromHours(24))
            .ToListAsync();
    }

    private IQueryable<UnitUnderTest> GetAllUnitsFromLastTimeStanQuery(
        HermesLocalContext ctx,
        TimeSpan? lastTimeSpan = null,
        int limit = 500)
    {
        var query = ctx.Set<UnitUnderTest>()
            .Include(x => x.SfcResponse)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit);

        if (lastTimeSpan != null)
        {
            var minDatetime = DateTime.Now - lastTimeSpan.Value;
            query = query.Where(x => x.CreatedAt >= minDatetime);
        }

        return query;
    }

    public async Task<List<UnitUnderTest>> FindBySerialNumberAsync(string serialNumber)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.Set<UnitUnderTest>()
            .Where(u => u.SerialNumber.Contains(serialNumber.ToUpper()))
            .ToListAsync();
    }
}