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

    public virtual async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var defect = await this._defectRepository.GetConsecutiveSameDefects(_maxConsecutiveDefects);
        if (!defect.IsNull)
        {
            return new Stop(StopType.Line, sfcResponse)
            {
                Defect = defect,
                Details =
                    $"{_maxConsecutiveDefects} consecutive defects in {defect.Location} with error code {defect.ErrorCode}"
            };
        }

        return Stop.Null;
    }
}