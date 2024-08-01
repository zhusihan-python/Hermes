using Hermes.Builders;
using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcResponseTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public SfcResponseTests(
        SfcResponseBuilder sfcResponseSfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._sfcResponseBuilder = sfcResponseSfcResponseBuilder;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    void IsFail_FailContent_ReturnsTrue()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetFailContent()
            .Build();
        Assert.True(sfcResponse.IsFail);
    }

    [Fact]
    void IsFail_PassContent_ReturnsFalse()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetOkContent()
            .Build();
        Assert.False(sfcResponse.IsFail);
    }

    [Fact]
    void ErrorType_TimeoutContent_ReturnsSfcErrorTypeTimeout()
    {
        var sfcResponse = this._sfcResponseBuilder.BuildTimeout();
        Assert.Equal(SfcErrorType.Timeout, sfcResponse.ErrorType);
    }

    [Fact]
    void ErrorType_WrongStationContent_ReturnsSfcErrorTypeTWrongStation()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetWrongStation()
            .Build();
        Assert.Equal(SfcErrorType.WrongStation, sfcResponse.ErrorType);
    }

    [Fact]
    void ErrorType_UnknownContent_ReturnsSfcErrorTypeTUnknown()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetUnknownContent()
            .Build();
        Assert.Equal(SfcErrorType.Unknown, sfcResponse.ErrorType);
    }

    [Fact]
    void IsRepair_RepairContent_ReturnsTrue()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetRepair(true)
            .Build();
        Assert.True(sfcResponse.IsRepair);
    }

    [Fact]
    void SerialNumber_ValidContent_ReturnsNotEmptyString()
    {
        var sfcResponse = this._sfcResponseBuilder
            .Build();
        Assert.False(string.IsNullOrEmpty(sfcResponse.SerialNumber));
    }

    [Fact]
    void Details_FailContent_ReturnsNotEmptyString()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetFailContent()
            .Build();
        Assert.False(string.IsNullOrEmpty(sfcResponse.Details));
    }

    [Fact]
    void Details_OkContent_ReturnsEmptyString()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetOkContent()
            .Build();
        Assert.True(string.IsNullOrEmpty(sfcResponse.Details));
    }

    [Fact]
    void IsNull_NotSfcResponseNull_ReturnsFalse()
    {
        var sfcResponse = this._sfcResponseBuilder.Build();
        Assert.False(sfcResponse.IsNull);
    }

    [Fact]
    void IsNull_IsSfcResponseNull_ReturnsTrue()
    {
        Assert.True(SfcResponse.Null.IsNull);
    }

    [Fact]
    void BuildTimeout_ValidUnitUnderTest_ReturnsSameUnitUnderTests()
    {
        var uut = this._unitUnderTestBuilder
            .Build();
        var sut = SfcResponse.BuildTimeout(uut);
        Assert.Equal(uut, sut.UnitUnderTest);
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
        var uut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        var sut = _sfcResponseBuilder
            .UnitUnderTest(uut)
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
        var uut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        var sut = _sfcResponseBuilder
            .UnitUnderTest(uut)
            .Build();
        Assert.True(sut.GetDefectByLocation(criticalLocation).IsNull);
    }

    [Fact]
    void GetDefectByLocation_WithMultipleCriticalDefect_ReturnsDefect()
    {
        const string criticalLocation = "L0";
        var defect = new Defect()
        {
            ErrorFlag = ErrorFlag.Bad,
            Location = criticalLocation,
        };
        var uut = this._unitUnderTestBuilder
            .AddDefect(defect)
            .Build();
        var sut = _sfcResponseBuilder
            .UnitUnderTest(uut)
            .Build();
        Assert.Equal(defect.Location, sut.GetDefectByLocation($"{criticalLocation.ToLower()},L1,L2").Location);
        Assert.Equal(defect.ErrorFlag, sut.GetDefectByLocation(criticalLocation).ErrorFlag);
    }

    [Fact]
    void GetDefectByLocation_WithoutDefects_ReturnsDefectNull()
    {
        const string criticalLocation = "L0";
        var sut = _sfcResponseBuilder.Build();
        Assert.True(sut.GetDefectByLocation(criticalLocation).IsNull);
    }
}