using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class AnyDefectsWithin1HourValidator : IStopValidator
{
    public const int DefaultMaxDefects = 3;

    private readonly IDefectRepository _defectRepository;
    private readonly int _maxDefects;

    public AnyDefectsWithin1HourValidator(
        IDefectRepository defectRepository,
        int maxDefects = DefaultMaxDefects)
    {
        this._defectRepository = defectRepository;
        this._maxDefects = maxDefects;
    }

    public virtual async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var defect = await this._defectRepository.GetAnyDefectsWithin1Hour(_maxDefects);
        if (!defect.IsNull)
        {
            return new Stop(StopType.Line, sfcResponse)
            {
                Defect = defect,
                Details = $"{_maxDefects} defects within 1 hour"
            };
        }

        return Stop.Null;
    }
}