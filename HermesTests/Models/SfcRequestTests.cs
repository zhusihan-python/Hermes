using Hermes.Builders;
using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcRequestTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private const string SfcPath = "/path/";
    private const FileExtension SfcResponseExtension = Hermes.Types.FileExtension.Ret;

    public SfcRequestTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public void FullPath_ValidDataGiven_Succeeds()
    {
        var unitUnderTest = _unitUnderTestBuilder.Build();
        var sfcRequest = new SfcRequest(unitUnderTest, SfcPath, SfcResponseExtension);

        Assert.Contains(SfcPath, sfcRequest.FullPath);
        Assert.Contains(unitUnderTest.FileName, sfcRequest.FullPath);
    }

    [Fact]
    public void ResponseFullPath_ValidDataGiven_Succeeds()
    {
        var unitUnderTest = _unitUnderTestBuilder.Build();
        var sfcRequest = new SfcRequest(unitUnderTest, SfcPath, SfcResponseExtension);

        Assert.Contains(SfcPath, sfcRequest.ResponseFullPath);
        Assert.Contains(Path.GetFileNameWithoutExtension(unitUnderTest.FileName), sfcRequest.ResponseFullPath);
        Assert.Contains(SfcResponseExtension.ToString().ToLower(), sfcRequest.ResponseFullPath);
    }
}