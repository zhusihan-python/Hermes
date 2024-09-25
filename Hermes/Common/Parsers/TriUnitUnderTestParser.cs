using Hermes.Models;
using Hermes.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hermes.Common.Parsers;

public class TriUnitUnderTestParser : IUnitUnderTestParser
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

    public string GetTestContent(string serialNumber, bool isPass, List<Defect>? defects)
    {
        var content = $"""
                       {serialNumber}
                       1105
                       110733
                       1A62FMM00-600-G+30-TOP
                       {(isPass ? "PASS" : "FAIL")}
                       210323220021
                       220323002539
                       NA
                       9
                       0
                       Error flag,Recipe name,Paste ID,CAD link Gerber,Error code,Multi Number 
                       """;
        if (defects == null) return content;
        foreach (var defect in defects)
        {
            content +=
                $"\n{(defect.ErrorFlag.ToString().ToUpper())};1A626AY00-600-G+A0-TOP;NA;{defect.Location};{defect.ErrorCode};1";
        }

        return content;
    }

    public Task<string> GetContentAsync(string content)
    {
        return Task.FromResult(content);
    }
}