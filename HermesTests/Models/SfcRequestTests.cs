using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcRequestTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder = new(new FileService(), new Settings());
    private const string SfcPath = "/path/";
    private const SfcResponseExtension SfcResponseExtension = Hermes.Types.SfcResponseExtension.RET;

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