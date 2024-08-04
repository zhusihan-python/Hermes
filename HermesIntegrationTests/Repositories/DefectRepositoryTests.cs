using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;

namespace HermesIntegrationTests.Repositories;

public class DefectRepositoryTests
{
    private readonly DefectRepository _sut;
    private readonly HermesContext _context;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public DefectRepositoryTests(UnitUnderTestBuilder unitUnderTestBuilder, HermesContext hermesContext)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._context = hermesContext;
        this._context.Database.EnsureDeleted();
        this._context.Database.EnsureCreated();
        this._sut = new DefectRepository(_context);
    }

    [Fact]
    public async void GetAnyNotRestoredDefectsWithin1Hour_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _context.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _context.SaveChangesAsync();

        _context.Stops.Add(new Stop()
        {
            Defects = _context.Defects.ToList(),
            IsRestored = false
        });

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetNotRestoredSameDefectsWithin1Hour_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _context.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _context.SaveChangesAsync();

        _context.Stops.Add(new Stop()
        {
            Defects = _context.Defects.ToList(),
            IsRestored = false
        });

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetNotRestoredConsecutiveSameDefects_AllDefectsAreRestored_ReturnsEmptyList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _context.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _context.SaveChangesAsync();

        _context.Stops.Add(new Stop()
        {
            Defects = _context.Defects.ToList(),
            IsRestored = false
        });

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetNotRestoredConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefectList()
    {
        const int qty = 3;
        var uutBuilder = _unitUnderTestBuilder
            .AddRandomDefect(isBad: true);
        for (var i = 0; i < qty; i++)
        {
            _context.UnitsUnderTest.Add(uutBuilder.Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(qty);
        Assert.Equal(qty, result.Count);
    }

    [Fact]
    public async void GetConsecutiveSameDefects_NotConsecutiveDefects_ReturnsEmptyList()
    {
        var uutBuilder = _unitUnderTestBuilder;
        var uutBuilderWithDefect = _unitUnderTestBuilder.Clone()
            .AddRandomDefect(isBad: true);
        _context.UnitsUnderTest.Add(uutBuilderWithDefect.Build());
        _context.UnitsUnderTest.Add(uutBuilderWithDefect.Build());
        _context.UnitsUnderTest.Add(uutBuilder.Build());
        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(3);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetSameDefectsWithin1Hour_WithSameDefectsWithin1Hour_ReturnsDefectList()
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
            _context.UnitsUnderTest.Add(uutBuilder.Build());
            _context.UnitsUnderTest.Add(uutBuilderWithDefects.Build());
            timeLapse -= step;
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Equal(qty, result.Count);
    }

    [Fact]
    public async void GetSameDefectsWithin1Hour_NotSameDefectsWithin1Hour_ReturnsEmptyList()
    {
        const int qty = 5;
        var timeLapse = TimeSpan.FromHours(1);
        var step = TimeSpan.FromMinutes(Math.Round(60.0 / qty));
        while (timeLapse.TotalMinutes > 0)
        {
            _context.UnitsUnderTest.Add(_unitUnderTestBuilder
                .Clone()
                .AddRandomDefect(isBad: true)
                .CreatedAt(DateTime.Now - timeLapse)
                .Build());
            timeLapse -= step;
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredSameDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetDefectsWithin1Hour_AnyDefectsWithin1Hour_ReturnsDefectList()
    {
        const int qty = 10;
        for (var i = 0; i < qty; i++)
        {
            _context.UnitsUnderTest.Add(_unitUnderTestBuilder
                .Clone()
                .AddRandomDefect(isBad: true)
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.True(result.Count >= qty);
    }

    [Fact]
    public async void GetDefectsWithin1Hour_NoDefectsWithin1Hour_ReturnsEmptyList()
    {
        _context.Defects.RemoveRange(_context.Defects);
        await _context.SaveChangesAsync();
        const int qty = 10;
        for (var i = 0; i <= qty; i++)
        {
            _context.UnitsUnderTest.Add(_unitUnderTestBuilder
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }
}