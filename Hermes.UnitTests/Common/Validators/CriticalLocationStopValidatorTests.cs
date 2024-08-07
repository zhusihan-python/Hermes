using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Moq;

namespace HermesTests.Common.Validators;

public class CriticalLocationStopValidatorTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public CriticalLocationStopValidatorTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public async Task ValidateAsync_WithCriticalDefects_ReturnStop()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            Location = criticalLocation
        };
        var unitUnderTest = GetUnitUnderTestMock(defect);
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        var result = await sut.ValidateAsync(unitUnderTest);
        Assert.False(result.IsNull);
        Assert.Equal(defect, result.Defects.First());
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsError_ReturnStopNull()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            Location = criticalLocation
        };
        var unitUnderTest = GetUnitUnderTestMock(defect);
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        var result = await sut.ValidateAsync(unitUnderTest);
        Assert.False(result.IsNull);
        Assert.Equal(defect, result.Defects.First());
    }

    [Fact]
    public async Task ValidateAsync_WithCriticalDefects_ReturnStopTypeLine()
    {
        var criticalLocation = "L0";
        var defect = new Defect()
        {
            ErrorFlag = ErrorFlag.Bad,
            Location = criticalLocation
        };
        var unitUnderTest = GetUnitUnderTestMock(defect, true);
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        Assert.Equal(StopType.Line, (await sut.ValidateAsync(unitUnderTest)).Type);
    }

    [Fact]
    public async Task ValidateAsync_NotCriticalDefects_ReturnStopNull()
    {
        var criticalLocation = "L0";
        var unitUnderTest = GetUnitUnderTestMock();
        var sut = new CriticalLocationStopValidator(new CoreSettings() { CriticalLocations = criticalLocation });
        Assert.True((await sut.ValidateAsync(unitUnderTest)).IsNull);
    }

    private UnitUnderTest GetUnitUnderTestMock(Defect? defect = null, bool isFail = false)
    {
        var mock = new Mock<UnitUnderTest>();
        mock
            .Setup(x => x.GetDefectByLocation(It.IsAny<string>()))
            .Returns(defect ?? Defect.Null);
        mock
            .Setup(x => x.IsFail)
            .Returns(isFail);
        return mock.Object;
    }
}