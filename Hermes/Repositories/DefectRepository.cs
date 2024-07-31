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
    public async Task<Defect> GetConsecutiveSameDefects(int lastUnitsUnderTest)
    {
        var defects = await this.GetFromLastSfcResponses(lastUnitsUnderTest)
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
            .GroupBy(x => new { x.Location, x.ErrorCode })
            .Select(x => new
            {
                Id = x.Max(defect => defect.Id),
                Count = x.Count()
            })
            .ToListAsync();

        var result = Defect.Null;
        foreach (var defect in defects.Where(defect => defect.Count >= lastUnitsUnderTest))
        {
            result = this.GetById(defect.Id) ?? Defect.Null;
        }

        return result;
    }

    private IQueryable<Defect> GetFromLastSfcResponses(int qty)
    {
        var uutIds = Db.SfcResponses
            .Where(x => x.ErrorType == SfcErrorType.None)
            .Take(qty)
            .OrderByDescending(x => x.UnitUnderTest.CreatedAt)
            .Select(x => x.UnitUnderTest.Id);
        return Db.Defects
            .Where(x => uutIds.Contains(x.UnitUnderTestId));
    }
}