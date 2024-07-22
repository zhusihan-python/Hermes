using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcRequestTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder = new(new FileService());
    private const string Path = "/path/";
    private const SfcResponseExtension SfcResponseExtension = Hermes.Types.SfcResponseExtension.RET;

    [Fact]
    public void FullPath_ValidDataGiven_Succeeds()
    {
        var unitUnderTest = _unitUnderTestBuilder.Build();
        var sfcRequest = new SfcRequest(unitUnderTest, Path, SfcResponseExtension);

        Assert.Contains(Path, sfcRequest.FullPath);
        Assert.Contains(unitUnderTest.FileName, sfcRequest.FullPath);
    }

    [Fact]
    public void ResponseFullPath_ValidDataGiven_Succeeds()
    {
        var unitUnderTest = _unitUnderTestBuilder.Build();
        var sfcRequest = new SfcRequest(unitUnderTest, Path, SfcResponseExtension);

        Assert.Contains(Path, sfcRequest.ResponseFullPath);
        Assert.Contains(unitUnderTest.FileName, sfcRequest.ResponseFullPath);
        Assert.Contains(SfcResponseExtension.ToString().ToLower(), sfcRequest.ResponseFullPath);
    }
}