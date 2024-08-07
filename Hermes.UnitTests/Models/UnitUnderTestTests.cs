using Hermes.Builders;
using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class UnitUnderTestTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public UnitUnderTestTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    void GetDefectByLocation_WithCriticalDefect_ReturnsDefect()
    {
        const string criticalLocation = "L0";
        var defect = new Defect()
        {
            ErrorFlag = ErrorFlag.Bad,
            Location = criticalLocation,
        };
        var sut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        Assert.Equal(defect.Location, sut.GetDefectByLocation(criticalLocation).Location);
        Assert.Equal(defect.ErrorFlag, sut.GetDefectByLocation(criticalLocation).ErrorFlag);
    }

    [Fact]
    void GetDefectByLocation_NotCriticalDefect_ReturnsDefectNull()
    {
        const string criticalLocation = "L0";
        var defect = new Defect()
        {
            ErrorFlag = ErrorFlag.Good,
            Location = criticalLocation,
        };
        var sut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        Assert.True(sut.GetDefectByLocation(criticalLocation).IsNull);
    }

    [Fact]
    void GetDefectByLocation_WithMultipleCriticalDefectLocations_ReturnsDefect()
    {
        const string criticalLocation = "L0";
        var defect = new Defect()
        {
            ErrorFlag = ErrorFlag.Bad,
            Location = criticalLocation,
        };
        var sut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        Assert.Equal(defect.Location, sut.GetDefectByLocation($"{criticalLocation.ToLower()},L1,L2").Location);
        Assert.Equal(defect.ErrorFlag, sut.GetDefectByLocation(criticalLocation).ErrorFlag);
    }

    [Fact]
    void GetDefectByLocation_WithoutDefects_ReturnsDefectNull()
    {
        const string criticalLocation = "L0";
        var sut = _unitUnderTestBuilder.Build();
        Assert.True(sut.GetDefectByLocation(criticalLocation).IsNull);
    }
}