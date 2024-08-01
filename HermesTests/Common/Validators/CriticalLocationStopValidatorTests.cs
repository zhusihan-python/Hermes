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
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        var result = await sut.ValidateAsync(sfcResponse);
        Assert.False(result.IsNull);
        Assert.Equal(defect, result.Defect);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsError_ReturnStopNull()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            Location = criticalLocation
        };
        var sfcResponse = GetSfcResponseMock(defect);
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
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
        var sfcResponse = GetSfcResponseMock(defect, true);
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_NotCriticalDefects_ReturnStopNull()
    {
        var criticalLocation = "L0";
        var sfcResponse = GetSfcResponseMock();
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    private SfcResponse GetSfcResponseMock(Defect? defect = null, bool isFail = false)
    {
        var mock = new Mock<SfcResponse>();
        mock
            .Setup(x => x.GetDefectByLocation(It.IsAny<string>()))
            .Returns(defect ?? Defect.Null);
        mock
            .Setup(x => x.IsFail)
            .Returns(isFail);
        return mock.Object;
    }
}