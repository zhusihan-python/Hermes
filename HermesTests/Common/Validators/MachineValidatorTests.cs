using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Types;

namespace HermesTests.Common.Validators;

public class MachineValidatorTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public MachineValidatorTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsSuccess_ReturnStopNull()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();

        var sut = new MachineStopValidator();
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsFail_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetFailContent()
            .Build();

        var sut = new MachineStopValidator();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsFail_ReturnStopTypeMachine()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetFailContent()
            .Build();

        var sut = new MachineStopValidator();
        Assert.Equal(StopType.Machine, (await sut.ValidateAsync(sfcResponse)).Type);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsTimeout_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetFailContent()
            .Build();

        var sut = new MachineStopValidator();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }
}