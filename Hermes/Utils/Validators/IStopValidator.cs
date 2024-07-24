using Hermes.Models;
using System.Threading.Tasks;

namespace Hermes.Utils.Validators;

public interface IStopValidator : IValidator<SfcResponse, Task<Stop>>
{
}