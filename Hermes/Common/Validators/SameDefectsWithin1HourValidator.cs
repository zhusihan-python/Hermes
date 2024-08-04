using System.Linq;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class SameDefectsWithin1HourValidator : IStopValidator
{
    public const int DefaultMaxSameDefects = 5;

    private readonly IDefectRepository _defectRepository;
    private readonly int _maxSameDefects;

    public SameDefectsWithin1HourValidator(
        IDefectRepository defectRepository,
        int maxSameDefects = DefaultMaxSameDefects)
    {
        this._defectRepository = defectRepository;
        this._maxSameDefects = maxSameDefects;
    }

    public virtual async Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var defects = await this._defectRepository.GetNotRestoredSameDefectsWithin1Hour(_maxSameDefects);
        if (defects.Count <= 0)
        {
            return Stop.Null;
        }

        var defect = defects.First();
        return new Stop(StopType.Line)
        {
            Defects = defects,
            Details =
                $"{_maxSameDefects} same defects within 1 hour in {defect.Location} with error code {defect.ErrorCode}"
        };
    }
}