using System.Collections.Generic;

namespace Hermes.Models.Parsers;

public interface IUnitUnderTestParser
{
    bool ParseIsFail(string content);
    string ParseSerialNumber(string content);
    List<Defect> ParseDefects(string content);
} 