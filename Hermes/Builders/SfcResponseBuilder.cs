using System;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Builders;

public class SfcResponseBuilder
{
    private const string PassContent = "OK\n{UuTSerialNumber}";
    private const string FailContent = "FAIL";
    private const string WrongStation = "GO-ICT\n{UuTSerialNumber}";
    private const string UnknownContent = "UNKNOWN";

    private string Content { get; set; } = UnknownContent;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public SfcResponseBuilder(UnitUnderTestBuilder unitUnderTestBuilderBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilderBuilder;
    }

    public SfcResponse Build()
    {
        var unitUnderTest = this._unitUnderTestBuilder.Build();
        return new SfcResponse(unitUnderTest, this.GetContent(unitUnderTest));
    }

    public string GetContent(UnitUnderTest unitUnderTest)
    {
        return this.Content.Replace("{UuTSerialNumber}", unitUnderTest.SerialNumber);
    }

    public SfcResponse BuildTimeout()
    {
        return SfcResponse.BuildTimeout(this._unitUnderTestBuilder.Build());
    }

    public SfcResponseBuilder SetOkContent()
    {
        this.Content = PassContent;
        return this;
    }

    public SfcResponseBuilder SetFailContent()
    {
        this.Content = FailContent;
        return this;
    }

    public SfcResponseBuilder SetWrongStation()
    {
        this.Content = WrongStation;
        return this;
    }

    public SfcResponseBuilder SetUnknownContent()
    {
        this.Content = UnknownContent;
        return this;
    }

    public SfcResponseBuilder AddBadDefect()
    {
        var random = new Random().Next();
        this._unitUnderTestBuilder.Defects.Add(new Defect()
        {
            ErrorFlag = ErrorFlag.Bad,
            ErrorCode = $"{random}x0000",
            Location = $"L{random}"
        });
        this._unitUnderTestBuilder.IsPass(false);
        return this;
    }

    public SfcResponseBuilder SetRepair(bool isRepair)
    {
        this._unitUnderTestBuilder.IsPass(!isRepair);
        this.SetOkContent();
        return this;
    }

    public SfcResponseBuilder Clone()
    {
        return new SfcResponseBuilder(this._unitUnderTestBuilder.Clone());
    }

    public SfcResponseBuilder CreatedAt(DateTime createdAt)
    {
        this._unitUnderTestBuilder.CreatedAt(createdAt);
        return this;
    }
}