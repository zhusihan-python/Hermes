using Hermes.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Hermes.Models.Parsers;

public class UnitUnderTestParser : IUnitUnderTestParser
{
    private const string GoodDefectText = "GOOD";
    private const RegexOptions RgxOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
    private static readonly Regex DefectRgx = new($"({GoodDefectText}|BAD);(.*);(.*);(.*);(.*);", RgxOptions);
    private static readonly Regex RegexIsFail = new(@"^fail[\r\n]+", RgxOptions);
    private static readonly Regex SerialNumberRgx = new(@"^\s*([A-z0-9-_]+)[\r\n]+");

    public List<Defect> ParseDefects(string content)
    {
        var defects = new List<Defect>();
        var matches = DefectRgx.Matches(content);
        foreach (Match match in matches)
        {
            var defect = new Defect()
            {
                ErrorFlag = match.Groups[1].Value == GoodDefectText ? ErrorFlag.Good : ErrorFlag.Bad,
                Location = match.Groups[4].Value,
                ErrorCode = match.Groups[5].Value
            };
            defects.Add(defect);
        }

        return defects;
    }

    public bool ParseIsFail(string content)
    {
        return RegexIsFail.IsMatch(content);
    }

    public string ParseSerialNumber(string content)
    {
        var match = SerialNumberRgx.Match(content);
        return match.Captures.Count > 0 ? match.Groups[1].Value : string.Empty;
    }
}