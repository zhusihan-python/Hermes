using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Types;

namespace HermesTests.Common.Validators;

public class MachineValidatorTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public MachineValidatorTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsSuccess_ReturnStopNull()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsPass(true)
            .IsSfcFail(false)
            .Build();

        var sut = new MachineStopValidator();
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsFail_ReturnStop()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcFail(true)
            .Build();

        var sut = new MachineStopValidator();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsFail_ReturnStopTypeMachine()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcFail(true)
            .Build();

        var sut = new MachineStopValidator();
        Assert.Equal(StopType.Machine, (await sut.ValidateAsync(sfcResponse)).Type);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsTimeout_ReturnStop()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcTimeout(true)
            .Build();

        var sut = new MachineStopValidator();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }
}