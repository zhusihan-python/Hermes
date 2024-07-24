using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Services;

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


    public SfcResponseBuilder IsPass(bool isPass)
    {
        this._unitUnderTest.IsPass(isPass);
        return this;
    }

    public SfcResponseBuilder UutFileName(string fileName)
    {
        this._unitUnderTest.FileName(fileName);
        return this;
    }

    public SfcResponseBuilder UutSerialNumber(string serialNumber)
    {
        this._unitUnderTest.SerialNumber(serialNumber);
        return this;
    }

    public SfcResponseBuilder UutIsPass(bool isPass)
    {
        this._unitUnderTest.IsPass(isPass);
        return this;
    }

    public SfcResponseBuilder UutDefect(Defect defect)
    {
        this._unitUnderTest.Defects.Add(defect);
        return this;
    }

    public string BuildContent()
    {
        var unitUnderTest = this._unitUnderTest.Build();
        return this.GetContent(unitUnderTest);
    }
}