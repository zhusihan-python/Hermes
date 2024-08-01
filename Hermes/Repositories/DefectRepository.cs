using System;
using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public sealed class DefectRepository(HermesContext db) : BaseRepository<Defect>(db), IDefectRepository
{
    public async Task<Defect> GetSameDefectsWithin1Hour(int qty)
    {
        return await GetRepeatedDefect(
            this.GetFromLastUnitsUnderTest(TimeSpan.FromHours(1)),
            qty);
    }

    public Task<Defect> GetAnyDefectsWithin1Hour(int qty)
    {
        var defects = this.GetFromLastUnitsUnderTest(TimeSpan.FromHours(1)).ToList();
        if (defects.Count >= qty)
        {
            return Task.FromResult(defects.Last());
        }

        return Task.FromResult(Defect.Null);
    }


    public async Task<Defect> GetConsecutiveSameDefects(int qty)
    {
        return await GetRepeatedDefect(
            this.GetFromLastUnitsUnderTest(qty),
            qty);
    }

    private async Task<Defect> GetRepeatedDefect(IQueryable<Defect> defectsQueryable, int qty)
    {
        var defects = await defectsQueryable
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
            .GroupBy(x => new { x.Location, x.ErrorCode })
            .Select(x => new
            {
                Id = x.Max(defect => defect.Id),
                Count = x.Count()
            })
            .ToListAsync();

        var result = Defect.Null;
        foreach (var defect in defects.Where(defect => defect.Count >= qty))
        {
            result = this.GetById(defect.Id) ?? Defect.Null;
        }

        return result;
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(TimeSpan fromHours)
    {
        var dateTimeLowerLimit = DateTime.Now - fromHours;
        var uutIds = Db.SfcResponses
            .Where(x => x.ErrorType == SfcErrorType.None)
            .Where(x => x.UnitUnderTest.CreatedAt > dateTimeLowerLimit)
            .OrderByDescending(x => x.UnitUnderTest.CreatedAt)
            .Select(x => x.UnitUnderTest.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }

    private IQueryable<Defect> GetFromLastUnitsUnderTest(int qty)
    {
        var uutIds = Db.SfcResponses
            .Where(x => x.ErrorType == SfcErrorType.None)
            .OrderByDescending(x => x.UnitUnderTest.CreatedAt)
            .Take(qty)
            .Select(x => x.UnitUnderTest.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }
}