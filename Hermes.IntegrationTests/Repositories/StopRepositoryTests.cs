using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;

namespace HermesIntegrationTests.Repositories;

public class StopRepositoryTests
{
    //private readonly StopRepository _sut;
    private readonly HermesLocalContext _localContext;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public StopRepositoryTests(
        SfcResponseBuilder sfcResponseBuilder,
        IDbContextFactory<HermesLocalContext> contextFactory)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._localContext = contextFactory.CreateDbContext();
        _localContext.Database.EnsureCreated();
        //this._sut = new StopRepository(contextFactory);
    }

    //[Fact]
    //public async Task GetConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefect()
    //{
    //    var sfcResponse = _sfcResponseBuilder.Build();
    //    await _localContext.SfcResponses.AddAsync(sfcResponse);
    //    await _localContext.SaveChangesAsync();
    //    var stop = new Stop(StopType.Machine);
    //    await this._sut.AddAndSaveAsync(stop);
    //    await this._sut.RestoreAsync(stop, new List<User>());
    //    var result = await this._sut.GetById(stop.Id);
    //    Assert.True(result?.IsRestored);
    //    Assert.NotEqual(0, result?.Id);
    //}
}