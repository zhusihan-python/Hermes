using Hermes.Models;
using System.Collections.Generic;

namespace Hermes.Utils.Parsers;

public interface IUnitUnderTestParser
{
    bool ParseIsFail(string content);
    string ParseSerialNumber(string content);
    List<Defect> ParseDefects(string content);
} 