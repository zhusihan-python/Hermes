using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;

namespace HermesTests.Models;

public class SfcResponseTests
{
    private readonly SfcResponseBuilder _builder = new(new UnitUnderTestBuilder(new FileService(), new Settings()));

    [Fact]
    void IsFail_FailContent_ReturnsTrue()
    {
        var sfcResponse = this._builder
            .SetFailContent()
            .Build();
        Assert.True(sfcResponse.IsFail);
    }

    [Fact]
    void IsFail_PassContent_ReturnsFalse()
    {
        var sfcResponse = this._builder
            .SetPassContent()
            .Build();
        Assert.False(sfcResponse.IsFail);
    }

    [Fact]
    void ErrorType_TimeoutContent_ReturnsSfcErrorTypeTimeout()
    {
        var sfcResponse = this._builder.BuildTimeout();
        Assert.Equal(SfcErrorType.Timeout, sfcResponse.ErrorType);
    }

    [Fact]
    void ErrorType_WrongStationContent_ReturnsSfcErrorTypeTWrongStation()
    {
        var sfcResponse = this._builder
            .SetWrongStation()
            .Build();
        Assert.Equal(SfcErrorType.WrongStation, sfcResponse.ErrorType);
    }

    [Fact]
    void ErrorType_UnknownContent_ReturnsSfcErrorTypeTUnknown()
    {
        var sfcResponse = this._builder
            .SetUnknownContent()
            .Build();
        Assert.Equal(SfcErrorType.Unknown, sfcResponse.ErrorType);
    }
}