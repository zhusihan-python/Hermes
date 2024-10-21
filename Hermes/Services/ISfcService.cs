using Hermes.Models;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public interface ISfcService
{
    Task<SfcResponse> SendAsync(UnitUnderTest unitUnderTest);
}