using System.Collections.Generic;
using Hermes.Types;

namespace Hermes.Common.Parsers;

public class ParserPrototype
{
    private readonly Dictionary<LogfileType, IUnitUnderTestParser> _parsersDictionary = new()
    {
        { LogfileType.TriDefault, new UnitUnderTestParser() }
    };

    public IUnitUnderTestParser? GetUnderTestParser(LogfileType logfileType)
    {
        return _parsersDictionary.TryGetValue(logfileType, out var parser) ? parser : null;
    }
}