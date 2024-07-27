using System.Collections.Generic;
using Hermes.Models;

namespace Hermes.Common.Parsers;

public interface IUnitUnderTestParser
{
    bool ParseIsFail(string content);
    string ParseSerialNumber(string content);
    List<Defect> ParseDefects(string content);
} 