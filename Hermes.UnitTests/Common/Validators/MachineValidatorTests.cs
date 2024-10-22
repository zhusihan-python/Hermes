using Hermes.Builders;
using Hermes.Common;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Moq;

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

        var sut = this.GetSut();
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsFail_ReturnStop()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcFail(true)
            .Build();

        var sut = this.GetSut();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsFail_ReturnStopTypeMachine()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcFail(true)
            .Build();

        var sut = this.GetSut();
        Assert.Equal(StopType.Machine, (await sut.ValidateAsync(sfcResponse)).Type);
    }

    [Fact]
    public async Task ValidateAsync_SfcResponseIsTimeout_ReturnStop()
    {
        var sfcResponse = _unitUnderTestBuilder
            .IsSfcTimeout(true)
            .Build();

        var sut = this.GetSut();
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    private MachineStopValidator GetSut(string additionalOkSfcResponse = "")
    {
        return new MachineStopValidator(new Settings()
        {
            AdditionalOkSfcResponse = additionalOkSfcResponse
        });
    }
}