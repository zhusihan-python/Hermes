using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;

namespace HermesIntegrationTests.Repositories;

public class DefectRepositoryTests
{
    private readonly DefectRepository _sut;
    private readonly HermesLocalContext _localContext;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public DefectRepositoryTests(
        UnitUnderTestBuilder unitUnderTestBuilder,
        IDbContextFactory<HermesLocalContext> contextFactory)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder
            .IsSfcFail(false);
        _localContext = contextFactory.CreateDbContext();
        _localContext.Database.EnsureCreated();
        this._sut = new DefectRepository(contextFactory);
    }

    [Fact]
    public async Task GetAnyNotRestoredDefectsWithin1Hour_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _localContext.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _localContext.SaveChangesAsync();

        _localContext.Stops.Add(new Stop()
        {
            Defects = _localContext.Defects.ToList(),
            IsRestored = false
        });

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNotRestoredSameDefectsWithin1Hour_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _localContext.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _localContext.SaveChangesAsync();

        _localContext.Stops.Add(new Stop()
        {
            Defects = _localContext.Defects.ToList(),
            IsRestored = false
        });

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNotRestoredConsecutiveSameDefects_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _localContext.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _localContext.SaveChangesAsync();

        _localContext.Stops.Add(new Stop()
        {
            Defects = _localContext.Defects.ToList(),
            IsRestored = false
        });

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNotRestoredConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefectList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _localContext.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(qty);
        Assert.Equal(qty, result.Count);
    }

    [Fact]
    public async Task GetConsecutiveSameDefects_NotConsecutiveDefects_ReturnsEmptyList()
    {
        var uutBuilder = _unitUnderTestBuilder;
        var uutBuilderWithDefect = _unitUnderTestBuilder.Clone()
            .AddRandomDefect(isBad: true);
        _localContext.UnitsUnderTest.Add(uutBuilderWithDefect.Build());
        _localContext.UnitsUnderTest.Add(uutBuilderWithDefect.Build());
        _localContext.UnitsUnderTest.Add(uutBuilder.Build());
        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(3);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSameDefectsWithin1Hour_WithSameDefectsWithin1Hour_ReturnsDefectList()
    {
        var uutBuilder = _unitUnderTestBuilder;
        var uutBuilderWithDefects = _unitUnderTestBuilder
            .Clone()
            .AddRandomDefect(isBad: true);
        const int qty = 5;
        var timeLapse = TimeSpan.FromMinutes(59);
        var step = timeLapse / qty;
        while (timeLapse.TotalMinutes > 0)
        {
            uutBuilder.CreatedAt(DateTime.Now - timeLapse);
            uutBuilderWithDefects.CreatedAt(DateTime.Now - timeLapse);
            await _localContext.UnitsUnderTest.AddAsync(uutBuilder.Build());
            await _localContext.UnitsUnderTest.AddAsync(uutBuilderWithDefects.Build());
            timeLapse -= step;
        }

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Equal(qty, result.Count);
    }

    [Fact]
    public async Task GetSameDefectsWithin1Hour_NotSameDefectsWithin1Hour_ReturnsEmptyList()
    {
        const int qty = 5;
        var timeLapse = TimeSpan.FromHours(1);
        var step = TimeSpan.FromMinutes(Math.Round(60.0 / qty));
        while (timeLapse.TotalMinutes > 0)
        {
            _localContext.UnitsUnderTest.Add(_unitUnderTestBuilder
                .Clone()
                .AddRandomDefect(isBad: true)
                .CreatedAt(DateTime.Now - timeLapse)
                .Build());
            timeLapse -= step;
        }

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDefectsWithin1Hour_AnyDefectsWithin1Hour_ReturnsDefectList()
    {
        const int qty = 10;
        for (var i = 0; i < qty; i++)
        {
            _localContext.UnitsUnderTest.Add(_unitUnderTestBuilder
                .Clone()
                .AddRandomDefect(isBad: true)
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.True(result.Count >= qty);
    }

    [Fact]
    public async Task GetDefectsWithin1Hour_NoDefectsWithin1Hour_ReturnsEmptyList()
    {
        _localContext.Defects.RemoveRange(_localContext.Defects);
        await _localContext.SaveChangesAsync();
        const int qty = 10;
        for (var i = 0; i <= qty; i++)
        {
            _localContext.UnitsUnderTest.Add(_unitUnderTestBuilder
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _localContext.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }
}