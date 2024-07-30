using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class ConsecutiveDefectsValidator : IStopValidator
{
    private const int MaxConsecutiveDefects = 3;
    private readonly DefectRepository _defectRepository;

    public ConsecutiveDefectsValidator(DefectRepository defectRepository)
    {
        this._defectRepository = defectRepository;
    }

    public async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var defects = await this._defectRepository
            .GetFromLastUnitsUnderTest(MaxConsecutiveDefects)
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
        foreach (var defect in defects.Where(defect => defect.Count >= MaxConsecutiveDefects))
        {
            result = new Stop(StopType.Line, sfcResponse)
            {
                Details =
                    $"{MaxConsecutiveDefects} Consecutive defects - Location: {defect.Location} ErrorCode: {defect.ErrorCode}"
            };
        }

        return result;
    }
}