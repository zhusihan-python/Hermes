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
            .Where(x => x.StopId == null)
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
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

        return Db.Defects.Where(x => ids.Contains(x.Id)).ToList();
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(TimeSpan fromHours)
    {
        var dateTimeLowerLimit = DateTime.Now - fromHours;
        var uutIds = Db.UnitsUnderTest
            .Where(x => x.SfcResponse != null && x.SfcResponse.ResponseType == SfcResponseType.Ok)
            .Where(x => x.CreatedAt >= dateTimeLowerLimit)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(int qty)
    {
        var uutIds = Db.UnitsUnderTest
            .Where(x => x.SfcResponse != null && x.SfcResponse.ResponseType == SfcResponseType.Ok)
            .OrderByDescending(x => x.CreatedAt)
            .Take(qty)
            .Select(x => x.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }
}