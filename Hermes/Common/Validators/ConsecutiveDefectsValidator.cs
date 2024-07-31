using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class ConsecutiveDefectsValidator : IStopValidator
{
    private const int MaxConsecutiveDefects = 3;
    private readonly IDefectRepository _defectRepository;

    public ConsecutiveDefectsValidator(IDefectRepository defectRepository)
    {
        this._defectRepository = defectRepository;
    }

    public async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var defect = await this._defectRepository.GetConsecutiveSameDefects(MaxConsecutiveDefects);
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