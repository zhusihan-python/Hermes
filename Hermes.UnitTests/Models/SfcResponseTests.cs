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
            .SetOkSfcResponse()
            .Build();
        Assert.False(sfcResponse.IsFail);
    }

    [Fact]
    void ErrorType_TimeoutContent_ReturnsSfcErrorTypeTimeout()
    {
        var sfcResponse = this._sfcResponseBuilder.BuildTimeout();
        Assert.Equal(SfcResponseType.Timeout, sfcResponse.ResponseType);
    }

    [Fact]
    void ErrorType_WrongStationContent_ReturnsSfcErrorTypeTWrongStation()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetWrongStation()
            .Build();
        Assert.Equal(SfcResponseType.WrongStation, sfcResponse.ResponseType);
    }

    [Fact]
    void ErrorType_UnknownContent_ReturnsSfcErrorTypeTUnknown()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetUnknownContent()
            .Build();
        Assert.Equal(SfcResponseType.Unknown, sfcResponse.ResponseType);
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
            .SetOkSfcResponse()
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
}