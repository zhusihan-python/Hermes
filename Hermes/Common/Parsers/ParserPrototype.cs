using Hermes.Types;
using System.Collections.Generic;
using Hermes.Repositories;

namespace Hermes.Common.Parsers;

public class ParserPrototype
{
    private readonly Dictionary<LogfileType, IUnitUnderTestParser> _parsersDictionary = new()
    {
        { LogfileType.TriDefault, new TriUnitUnderTestParser() }
    };

    public ParserPrototype(
        LabelingMachineUnitUnderTestParser labelingMachineUnitUnderTestParser,
        GkgUnitUnderTestParser gkgUnitUnderTestParser)
    {
        this._parsersDictionary.Add(LogfileType.LabelingMachineDefault,
            labelingMachineUnitUnderTestParser);
        this._parsersDictionary.Add(LogfileType.GkgDefault,
            gkgUnitUnderTestParser);
    }

    public IUnitUnderTestParser? GetUnitUnderTestParser(LogfileType logfileType)
    {
        return _parsersDictionary.TryGetValue(logfileType, out var parser) ? parser : null;
    }
}