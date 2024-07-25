using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcResponseTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public SfcResponseTests(SfcResponseBuilder sfcResponseSfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseSfcResponseBuilder;
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
            .SetPassContent()
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
}