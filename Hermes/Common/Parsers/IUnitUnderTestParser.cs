using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Common.Parsers;

public interface IUnitUnderTestParser
{
    bool ParseIsFail(string content);
    string ParseSerialNumber(string content);
    List<Defect> ParseDefects(string content);
    string GetTestContent(string serialNumber, bool isPass, List<Defect>? defects = null);
    Task<string> GetContentAsync(string content);
}