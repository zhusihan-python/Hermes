using Hermes.Common.Extensions;
using Hermes.Types;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hermes.Models;

public class SfcResponse
{
    public static readonly SfcResponse Null = new SfcResponseNull();

    private const RegexOptions RgxOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
    private static readonly Regex RegexWrongStation = new(@"^go-.+[\r\n]+", RgxOptions);
    private static readonly Regex RegexIsOk = new(@"^ok[\r\n]+", RgxOptions);
    private const string TimeoutText = "Timeout";

    [Key] public int Id { get; init; }
    public UnitUnderTest UnitUnderTest { get; init; } = UnitUnderTest.Null;
    public int UnitUnderTestId { get; init; }
    public bool IsFail => this.ErrorType != SfcErrorType.None;
    public SfcErrorType ErrorType { get; init; }
    [MaxLength(3000)] public string Content { get; init; } = "";
    [NotMapped] public bool IsRepair => this.UnitUnderTest?.IsFail ?? true && !this.IsFail;
    [NotMapped] public string SerialNumber => UnitUnderTest?.SerialNumber ?? string.Empty;
    [NotMapped] public string Details => IsFail ? $"{ErrorType} - {ErrorType.GetDescription()}" : "";
    [NotMapped] public bool IsNull => this == Null;

    public SfcResponse()

    {
    }

    public SfcResponse(UnitUnderTest unitUnderTest, string content)
    {
        this.UnitUnderTest = unitUnderTest;
        this.UnitUnderTestId = unitUnderTest.Id;
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

    public static SfcResponse BuildTimeout(UnitUnderTest unitUnderTest)
    {
        return new SfcResponse(unitUnderTest, TimeoutText);
    }

    public virtual Defect GetDefectByLocation(string criticalLocations)
    {
        return this.UnitUnderTest.GetDefectByLocation(criticalLocations);
    }
}

public class SfcResponseNull() : SfcResponse();