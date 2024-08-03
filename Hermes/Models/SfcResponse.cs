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
    public virtual bool IsFail => this.ResponseType != SfcResponseType.Ok;
    public SfcResponseType ResponseType { get; init; }
    [MaxLength(3000)] public string Content { get; init; } = "";
    [NotMapped] public bool IsRepair => this.UnitUnderTest?.IsFail ?? true && !this.IsFail;
    [NotMapped] public string SerialNumber => UnitUnderTest?.SerialNumber ?? string.Empty;
    [NotMapped] public string Details => IsFail ? $"{ResponseType} - {ResponseType.GetDescription()}" : "";
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsTimeout => ResponseType == SfcResponseType.Timeout;

    public SfcResponse()

    {
    }

    public SfcResponse(UnitUnderTest unitUnderTest, string content)
    {
        this.UnitUnderTest = unitUnderTest;
        this.UnitUnderTestId = unitUnderTest.Id;
        this.Content = content;
        this.ResponseType = ParseErrorType(content);
    }

    private static SfcResponseType ParseErrorType(string content)
    {
        if (RegexIsOk.Match(content).Success)
        {
            return SfcResponseType.Ok;
        }

        if (RegexWrongStation.Match(content).Success)
        {
            return SfcResponseType.WrongStation;
        }

        if (content == TimeoutText)
        {
            return SfcResponseType.Timeout;
        }

        return SfcResponseType.Unknown;
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