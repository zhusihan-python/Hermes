using Hermes.Models;

namespace Hermes.Builders;

public class SfcResponseBuilder
{
    private const string PassContent = "OK\n{UuTSerialNumber}";
    private const string FailContent = "FAIL";
    private const string WrongStation = "GO-ICT\n{UuTSerialNumber}";
    private const string UnknownContent = "UNKNOWN";

    private string Content { get; set; } = UnknownContent;
    private readonly UnitUnderTestBuilder _unitUnderTest;

    public SfcResponseBuilder(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTest = unitUnderTestBuilder;
    }

    public SfcResponse Build()
    {
        var unitUnderTest = this._unitUnderTest.Build();
        return new SfcResponse(unitUnderTest, this.GetContent(unitUnderTest));
    }

    public string GetContent(UnitUnderTest unitUnderTest)
    {
        return this.Content.Replace("{UuTSerialNumber}", unitUnderTest.SerialNumber);
    }

    public SfcResponse BuildTimeout()
    {
        return SfcResponse.BuildTimeout(this._unitUnderTest.Build());
    }

    public SfcResponseBuilder SetPassContent()
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
}