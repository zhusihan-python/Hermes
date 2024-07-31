using Hermes.Builders;
using Hermes.Common.Validators;

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
            .SetPassContent()
            .Build();

        var validator = new MachineStopValidator();
        Assert.True((await validator.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsFail_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetFailContent()
            .Build();

        var validator = new MachineStopValidator();
        Assert.False((await validator.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_SfcResponseIsTimeout_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetFailContent()
            .Build();

        var validator = new MachineStopValidator();
        Assert.False((await validator.ValidateAsync(sfcResponse)).IsNull);
    }
}