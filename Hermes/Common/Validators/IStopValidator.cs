using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Common.Validators;

public interface IStopValidator : IValidator<SfcResponse, Task<Stop>>
{
}