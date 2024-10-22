using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Repositories;
using Moq;

namespace HermesTests.Common.Parsers;

public class LabelingMachineUnitUnderTestParserTests
{
    private readonly LabelingMachineUnitUnderTestParser _sut;

    public LabelingMachineUnitUnderTestParserTests()
    {
        this._sut = BuildSut();
    }

    [Fact]
    public void ParseDefects_Called_ReturnsEmptyList()
    {
        Assert.Empty(this._sut.ParseDefects(string.Empty));
    }

    [Fact]
    public void ParseIsFail_Called_ReturnsFalse()
    {
        Assert.False(this._sut.ParseIsFail(string.Empty));
    }

    [Fact]
    public void ParseSerialNumber_ValidSerialNumber_ReturnsSerialNumberIsParsed()
    {
        const string serialNumber = "1A62FMM00403397RM";
        var content = _sut.GetTestContent(serialNumber, true);
        Assert.Equal(serialNumber, this._sut.ParseSerialNumber(content));
    }

    [Fact]
    public void ParseSerialNumber_InvalidSerialNumber_ReturnsEmptyString()
    {
        const string serialNumber = "@";
        var content = _sut.GetTestContent(serialNumber, true);
        Assert.Empty(this._sut.ParseSerialNumber(content));
    }

    [Fact]
    public async Task GetContent_ValidPackageId_ReturnsContentWithPackageId()
    {
        const string pkgId = "S123456789";
        var sut = BuildSut(new Package() { Id = pkgId });
        const string serialNumber = "1A62FMM00403397RM";
        var content = sut.GetTestContent(serialNumber, true);
        Assert.Contains(pkgId, await sut.GetContentAsync(content));
    }

    [Fact]
    public async Task GetContent_NotValidPackageId_ReturnsContentWithoutPackageId()
    {
        var sut = BuildSut();
        const string serialNumber = "1A62FMM00403397RM";
        var content = sut.GetTestContent(serialNumber, true);
        Assert.Contains(LabelingMachineUnitUnderTestParser.NoPackageAvailableText, await sut.GetContentAsync(content));
    }

    [Fact]
    public async Task GetContent_EmptyContent_ReturnsEmptyString()
    {
        Assert.Empty(await _sut.GetContentAsync(string.Empty));
    }

    private LabelingMachineUnitUnderTestParser BuildSut(Package? package = null)
    {
        var mockSfcRepository = new Mock<ISfcRepository>();
        mockSfcRepository.Setup(x => x.FindNextCanUsePackage(It.IsAny<string>()))
            .ReturnsAsync(package ?? Package.Null);
        var sut = new LabelingMachineUnitUnderTestParser(mockSfcRepository.Object, new Settings());
        return sut;
    }
}