using Hermes.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hermes.Models;

public class SfcResponse
{
    private static readonly Regex RegexWrongStation =
        new(@"^go-.+[\r\n]+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static readonly Regex RegexIsOk = new(@"^ok[\r\n]+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private const string TimeoutText = "Timeout";

    [Key] public int Id { get; init; }
    public UnitUnderTest? UnitUnderTest { get; init; }
    public int UnitUnderTestId { get; init; }
    [MaxLength(1000)] public string Content { get; init; } = "";
    public bool IsFail => this.ErrorType != SfcErrorType.None;
    public SfcErrorType ErrorType { get; init; }
    [NotMapped] public bool UsunitUnderTestFail => this.UnitUnderTest?.IsFail ?? false;
    [NotMapped] public string UnitUnderTestSerialNumber => UnitUnderTest?.SerialNumber ?? string.Empty;

    [NotMapped] public string Details => IsFail ? $"{ErrorType}" : "";

    public SfcResponse()
    {
    }

    public SfcResponse(UnitUnderTest logfile, string content)
    {
        this.UnitUnderTest = logfile;
        this.Content = content;
        this.ErrorType = ParseErrorType(content);
    }

    private static SfcErrorType ParseErrorType(string content)
    {
        if (RegexIsOk.Match(content).Success)
        {
            return SfcErrorType.None;
        }

        if (RegexWrongStation.Match(content).Success)
        {
            return SfcErrorType.WrongStation;
        }

        if (content == TimeoutText)
        {
            return SfcErrorType.Timeout;
        }

        return SfcErrorType.Unknown;
    }

    public static SfcResponse BuildTimeout(UnitUnderTest logfile)
    {
        return new SfcResponse(logfile, TimeoutText);
    }
}