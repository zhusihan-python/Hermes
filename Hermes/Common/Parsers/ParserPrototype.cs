using Hermes.Types;
using System.Collections.Generic;
using Hermes.Repositories;

namespace Hermes.Common.Parsers;

public class ParserPrototype
{
    private readonly Dictionary<LogfileType, IUnitUnderTestParser> _parsersDictionary = new()
    {
        { LogfileType.TriDefault, new TriUnitUnderTestParser() },
        { LogfileType.GkgDefault, new GkgUnitUnderTestParser() },
    };

    public ParserPrototype(LabelingMachineUnitUnderTestParser labelingMachineUnitUnderTestParser)
    {
        this._parsersDictionary.Add(LogfileType.LabelingMachineDefault,
            labelingMachineUnitUnderTestParser);
    }

    public IUnitUnderTestParser? GetUnitUnderTestParser(LogfileType logfileType)
    {
        return _parsersDictionary.TryGetValue(logfileType, out var parser) ? parser : null;
    }
}