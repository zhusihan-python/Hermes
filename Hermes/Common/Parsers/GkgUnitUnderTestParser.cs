using System;
using Hermes.Models;
using Hermes.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hermes.Common.Parsers;

public class GkgUnitUnderTestParser : IUnitUnderTestParser
{
    private const RegexOptions RgxOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
    private static readonly Regex SerialNumberRgx = new(@"Describe,Value[\r\n]*,*[\r\n]*([\w-_']+),{1}", RgxOptions);

    public List<Defect> ParseDefects(string content)
    {
        return new List<Defect>();
    }

    public bool ParseIsFail(string content)
    {
        return false;
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
                       6121
                       gkg
                       SSD-4TB-A67
                       {(isPass ? "PASS" : "FAIL")}
                       {DateTime.Now:ddMMyyHHmmss}
                       {DateTime.Now:ddMMyyHHmmss}
                       NA
                       0
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