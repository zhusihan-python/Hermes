using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;

namespace HermesIntegrationTests.Repositories;

public class StopRepositoryTests
{
    private readonly StopRepository _sut;
    private readonly HermesLocalContext _localContext;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public StopRepositoryTests(SfcResponseBuilder sfcResponseBuilder, HermesLocalContext hermesLocalContext)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._localContext = hermesLocalContext;
        this._sut = new StopRepository(_localContext);
    }

    [Fact]
    public async Task GetConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefect()
    {
        var sfcResponse = _sfcResponseBuilder.Build();
        await _localContext.SfcResponses.AddAsync(sfcResponse);
        await _localContext.SaveChangesAsync();
        var stop = new Stop(StopType.Machine);
        await this._sut.AddAndSaveAsync(stop);
        await this._sut.RestoreAsync(stop);
        Assert.True(stop.IsRestored);
        Assert.NotEqual(0, stop.Id);
    }
}