using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class SameDefectsWithin1HourValidator : IStopValidator
{
    public const int DefaultMaxSameDefects = 3;

    private readonly IDefectRepository _defectRepository;
    private readonly int _maxSameDefects;

    public SameDefectsWithin1HourValidator(
        IDefectRepository defectRepository,
        int maxSameDefects = DefaultMaxSameDefects)
    {
        this._defectRepository = defectRepository;
        this._maxSameDefects = maxSameDefects;
    }

    public virtual async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var defect = await this._defectRepository.GetSameDefectsWithin1Hour(_maxSameDefects);
        if (!defect.IsNull)
        {
            return new Stop(StopType.Line, sfcResponse)
            {
                Defect = defect
            };
        }

        return Stop.Null;
    }
}