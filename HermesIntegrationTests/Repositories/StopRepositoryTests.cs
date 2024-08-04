using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;

namespace HermesIntegrationTests.Repositories;

public class StopRepositoryTests
{
    private readonly StopRepository _sut;
    private readonly HermesContext _context;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public StopRepositoryTests(SfcResponseBuilder sfcResponseBuilder, HermesContext hermesContext)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._context = hermesContext;
        this._sut = new StopRepository(_context);
    }

    [Fact]
    public async void GetConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefect()
    {
        var sfcResponse = _sfcResponseBuilder.Build();
        await _context.SfcResponses.AddAsync(sfcResponse);
        await _context.SaveChangesAsync();
        var stop = new Stop(StopType.Machine);
        await this._sut.AddAndSaveAsync(stop);
        await this._sut.RestoreAsync(stop);
        Assert.True(stop.IsRestored);
        Assert.NotEqual(0, stop.Id);
    }
}