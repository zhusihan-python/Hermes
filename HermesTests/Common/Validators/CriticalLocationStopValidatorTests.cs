using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Moq;

namespace HermesTests.Common.Validators;

public class CriticalLocationStopValidatorTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public CriticalLocationStopValidatorTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public async void ValidateAsync_WithCriticalDefects_ReturnStop()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            Location = criticalLocation
        };
        var sfcResponse = GetSfcResponseMock(defect);
        var sut = new CriticalLocationStopValidator(criticalLocation);
        var result = await sut.ValidateAsync(sfcResponse);
        Assert.False(result.IsNull);
        Assert.Equal(defect, result.Defect);
    }

    [Fact]
    public async void ValidateAsync_WithCriticalDefects_ReturnStopTypeLine()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            Location = criticalLocation
        };
        var sfcResponse = GetSfcResponseMock(defect);
        var sut = new CriticalLocationStopValidator(criticalLocation);
        Assert.Equal(StopType.Line, (await sut.ValidateAsync(sfcResponse)).Type);
    }

    [Fact]
    public async void ValidateAsync_NotCriticalDefects_ReturnStopNull()
    {
        var criticalLocation = "L0";
        var sfcResponse = GetSfcResponseMock();
        var sut = new CriticalLocationStopValidator(criticalLocation);
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    private SfcResponse GetSfcResponseMock(Defect? defect = null)
    {
        var mock = new Mock<SfcResponse>();
        mock
            .Setup(x => x.GetDefectByLocation(It.IsAny<string>()))
            .Returns(defect ?? Defect.Null);
        return mock.Object;
    }
}