using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Moq;

namespace HermesTests.Common.Validators;

public class RuleThreeFiveTenValidatorTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public RuleThreeFiveTenValidatorTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public async void ValidateAsync_NotConsecutiveDefects_ReturnStopNull()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var sut = BuildSut(consecutiveDefectsStop: Stop.Null);

        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_WithConsecutiveDefects_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(consecutiveDefectsStop: stop);

        Assert.Equal(stop, await sut.ValidateAsync(sfcResponse));
    }

    [Fact]
    public async void ValidateAsync_WithConsecutiveDefects_ReturnStopTypeLine()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(consecutiveDefectsStop: stop);

        Assert.Equal(stop, await sut.ValidateAsync(sfcResponse));
    }

    [Fact]
    public async void ValidateAsync_NotSameDefectsWithin1Hour_ReturnStopNull()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(sameDefectsWithin1HourStop: stop);

        Assert.Equal(StopType.Line, (await sut.ValidateAsync(sfcResponse)).Type);
    }

    [Fact]
    public async void ValidateAsync_WithSameDefectsWithin1Hour_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(sameDefectsWithin1HourStop: stop);

        Assert.Equal(stop, await sut.ValidateAsync(sfcResponse));
    }

    [Fact]
    public async void ValidateAsync_NotAnyDefectsWithin1Hour_ReturnStopNull()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var sut = BuildSut(anyDefectsWithin1HourStop: Stop.Null);

        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_WithAnyDefectsWithin1Hour_ReturnStop()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(anyDefectsWithin1HourStop: stop);

        Assert.Equal(stop, await sut.ValidateAsync(sfcResponse));
    }

    [Fact]
    public async void ValidateAsync_UnitUnderTest_ReturnStopNull()
    {
        var sfcResponse = _sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Line, sfcResponse);
        var sut = BuildSut(anyDefectsWithin1HourStop: stop);

        Assert.Equal(stop, await sut.ValidateAsync(sfcResponse));
    }

    private RuleThreeFiveTenValidator BuildSut(
        Stop? consecutiveDefectsStop = null,
        Stop? sameDefectsWithin1HourStop = null,
        Stop? anyDefectsWithin1HourStop = null)
    {
        return new RuleThreeFiveTenValidator(
            GetValidatorMock<ConsecutiveDefectsValidator>(consecutiveDefectsStop),
            GetValidatorMock<SameDefectsWithin1HourValidator>(sameDefectsWithin1HourStop),
            GetValidatorMock<AnyDefectsWithin1HourValidator>(anyDefectsWithin1HourStop));
    }

    private T GetValidatorMock<T>(Stop? stop = null) where T : class, IStopValidator
    {
        var defectRepository = new Mock<IDefectRepository>();
        var mock = new Mock<T>(
            defectRepository.Object,
            ConsecutiveDefectsValidator.DefaultMaxConsecutiveDefects);
        mock
            .Setup(x => x.ValidateAsync(It.IsAny<SfcResponse>()))
            .Returns(Task.FromResult(stop ?? Stop.Null));
        return mock.Object;
    }
}