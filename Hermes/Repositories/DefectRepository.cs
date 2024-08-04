using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public sealed class DefectRepository(HermesContext db) : BaseRepository<Defect>(db), IDefectRepository
{
    public Task<List<Defect>> GetAnyNotRestoredDefectsWithin1Hour(int qty)
    {
        var defects = this.GetFromLastUnitsUnderTest(TimeSpan.FromHours(1)).ToList();
        return defects.Count >= qty
            ? Task.FromResult(defects.ToList())
            : Task.FromResult(new List<Defect>());
    }

    public async Task<List<Defect>> GetNotRestoredSameDefectsWithin1Hour(int qty)
    {
        return await GetNotRestoredRepeatedDefects(
            this.GetFromLastUnitsUnderTest(TimeSpan.FromHours(1)),
            qty);
    }

    public async Task<List<Defect>> GetNotRestoredConsecutiveSameDefects(int qty)
    {
        return await GetNotRestoredRepeatedDefects(
            this.GetFromLastUnitsUnderTest(qty),
            qty);
    }

    private async Task<List<Defect>> GetNotRestoredRepeatedDefects(IQueryable<Defect> defectsQueryable, int qty)
    {
        var defects = await defectsQueryable
            .Join(
                db.Stop,
                defect => defect.StopId,
                stop => stop.Id,
                (defect, stop) => new { defect, stop })
            .Where(x => x.defect.ErrorFlag == ErrorFlag.Bad)
            .GroupBy(x => new { x.defect.Location, x.defect.ErrorCode })
            .Select(x => new
            {
                Ids = string.Join(",", x.Select(y => y.defect.Id)),
                Count = x.Count()
            })
            .ToListAsync();

        var ids = Array.Empty<int>();
        foreach (var defect in defects.Where(defect => defect.Count >= qty))
        {
            ids = defect.Ids.Split(',').Select(int.Parse).ToArray();
        }

        return Db.Defects.Where(x => ids.Contains(x.Id)).ToList();
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(TimeSpan fromHours)
    {
        var dateTimeLowerLimit = DateTime.Now - fromHours;
        var uutIds = Db.SfcResponses
            .Where(x => x.ResponseType == SfcResponseType.Ok)
            .Where(x => x.UnitUnderTest.CreatedAt >= dateTimeLowerLimit)
            .OrderByDescending(x => x.UnitUnderTest.CreatedAt)
            .Select(x => x.UnitUnderTest.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(int qty)
    {
        var uutIds = Db.SfcResponses
            .Where(x => x.ResponseType == SfcResponseType.Ok)
            .OrderByDescending(x => x.UnitUnderTest.CreatedAt)
            .Take(qty)
            .Select(x => x.UnitUnderTest.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }
}