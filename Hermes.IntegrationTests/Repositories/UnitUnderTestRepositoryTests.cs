using Hermes.Builders;
using Hermes.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HermesIntegrationTests.Repositories;

public class UnitUnderTestRepositoryTests
{
    private readonly UnitUnderTestRepository _sut;
    private readonly HermesLocalContext _localContext;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public UnitUnderTestRepositoryTests(
        UnitUnderTestBuilder unitUnderTestBuilder,
        IDbContextFactory<HermesLocalContext> contextFactory)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._localContext = contextFactory.CreateDbContext();
        this._sut = new UnitUnderTestRepository(contextFactory);
    }

    [Fact]
    public async Task AddAndSaveAsync_ValidUnitUnderTest_AddsAndSaves()
    {
        var uut = _unitUnderTestBuilder.Build();
        await _sut.AddAndSaveAsync(uut);
        var result = await _sut.GetById(uut.Id);
        Assert.NotNull(result);
        Assert.Equal(uut.Id, result.Id);
    }

    [Fact]
    public async Task Delete_ValidUnitUnderTest_Deletes()
    {
        var uut = _unitUnderTestBuilder.Build();
        await _sut.AddAndSaveAsync(uut);
        await _sut.Delete(uut.Id);
        var result = await _sut.GetById(uut.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_ValidUnitUnderTest_GetsAll()
    {
        const int listLength = 3;
        for (int i = 0; i < listLength; i++)
        {
            var uut = _unitUnderTestBuilder.Build();
            await _sut.AddAndSaveAsync(uut);
        }

        Assert.True(listLength <= (await _sut.GetAll()).Count);
    }
}