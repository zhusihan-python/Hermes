using System;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Common.Validators;

public class StopLineValidator : IStopValidator
{
    private readonly DefectRepository _defectRepository;

    public StopLineValidator(DefectRepository defectRepository)
    {
        this._defectRepository = defectRepository;
    }

    public async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        // TODO: ConsecutiveSameFailures (Error flag == bad)
        var defects = await this._defectRepository
            .GetFromLastUnitsUnderTest(3)
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
            .GroupBy(x => new { x.Location, x.ErrorCode })
            .Select(x => new
            {
                Location = x.Key.Location,
                ErrorCode = x.Key.ErrorCode,
                Count = x.Count()
            })
            .ToListAsync();
        var result = Stop.Null;
        foreach (var defect in defects.Where(defect => defect.Count >= 3))
        {
            result = new Stop(StopType.Line, sfcResponse);
        }

        // TODO: SameFailuresWithin1Hour
        // TODO: FailuresWithin1Hour
        return result;
    }
}