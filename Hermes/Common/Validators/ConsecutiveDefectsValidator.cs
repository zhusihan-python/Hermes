using System.Linq;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class ConsecutiveDefectsValidator : IStopValidator
{
    public const int DefaultMaxConsecutiveDefects = 3;

    private readonly IDefectRepository _defectRepository;
    private readonly int _maxConsecutiveDefects;

    public ConsecutiveDefectsValidator(
        IDefectRepository defectRepository,
        int maxConsecutiveDefects = DefaultMaxConsecutiveDefects)
    {
        this._defectRepository = defectRepository;
        this._maxConsecutiveDefects = maxConsecutiveDefects;
    }

    public virtual async Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var defects = await this._defectRepository.GetNotRestoredConsecutiveSameDefects(_maxConsecutiveDefects);
        if (defects.Count <= 0)
        {
            return Stop.Null;
        }

        var defect = defects.First();
        return new Stop(StopType.Line)
        {
            Defects = defects,
            Details =
                $"{_maxConsecutiveDefects} consecutive defects in {defect.Location} with error code {defect.ErrorCode}"
        };
    }
}