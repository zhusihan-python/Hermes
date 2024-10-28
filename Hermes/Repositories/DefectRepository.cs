using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public sealed class DefectRepository : BaseRepository<UnitUnderTest, HermesLocalContext>, IDefectRepository
{
    private readonly IDbContextFactory<HermesLocalContext> _context;

    public DefectRepository(IDbContextFactory<HermesLocalContext> context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Defect>> GetAnyNotRestoredDefectsWithin1Hour(int qty)
    {
        var ctx = await _context.CreateDbContextAsync();
        var defects = await this.GetFromLastUnitsUnderTest(ctx, fromHours: TimeSpan.FromHours(1)).ToListAsync();
        return defects.Count >= qty
            ? defects.ToList()
            : [];
    }

    public async Task<List<Defect>> GetNotRestoredSameDefectsWithin1Hour(int qty)
    {
        var ctx = await _context.CreateDbContextAsync();
        return await GetNotRestoredRepeatedDefects(
            this.GetFromLastUnitsUnderTest(ctx, fromHours: TimeSpan.FromHours(1)),
            qty);
    }

    public async Task<List<Defect>> GetNotRestoredConsecutiveSameDefects(int qty)
    {
        var ctx = await _context.CreateDbContextAsync();
        return await GetNotRestoredRepeatedDefects(
            this.GetFromLastUnitsUnderTest(ctx, qty),
            qty);
    }

    private async Task<List<Defect>> GetNotRestoredRepeatedDefects(IQueryable<Defect> defectsQueryable, int qty)
    {
        var defects = await defectsQueryable
            .GroupBy(x => new { x.Location, x.ErrorCode })
            .Select(x => new
            {
                Ids = string.Join(",", x.Select(y => y.Id)),
                Count = x.Count()
            })
            .ToListAsync();

        var ids = Array.Empty<int>();
        foreach (var defect in defects.Where(defect => defect.Count >= qty))
        {
            ids = defect.Ids.Split(',').Select(int.Parse).ToArray();
        }

        var ctx = await _context.CreateDbContextAsync();
        return ctx.Defects.Where(x => ids.Contains(x.Id)).ToList();
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(HermesLocalContext ctx, int? qty = null,
        TimeSpan? fromHours = null)
    {
        var uutQuery = ctx.UnitsUnderTest
            .Where(x => x.SfcResponse != null && x.SfcResponse.Type == SfcResponseType.Ok)
            .OrderByDescending(x => x.Id)
            .AsQueryable();

        if (fromHours != null)
        {
            var dateTimeLowerLimit = DateTime.Now - fromHours;
            uutQuery = uutQuery.Where(x => x.CreatedAt >= dateTimeLowerLimit);
        }

        if (qty != null)
        {
            uutQuery = uutQuery.Take(qty.GetValueOrDefault());
        }

        var uutIds = uutQuery
            .Select(x => x.Id);
        return ctx.Defects
            .Where(x => x.StopId == null)
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }
}