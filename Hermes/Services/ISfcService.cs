using Hermes.Models;
using System.Threading.Tasks;

namespace Hermes.Services;

public interface ISfcService
{
    Task<SfcResponse> SendAsync(UnitUnderTest unitUnderTest);
}