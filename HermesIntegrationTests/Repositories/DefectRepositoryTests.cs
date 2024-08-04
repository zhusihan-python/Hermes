using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;

namespace HermesIntegrationTests.Repositories;

public class DefectRepositoryTests
{
    private readonly DefectRepository _sut;
    private readonly HermesContext _context;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public DefectRepositoryTests(SfcResponseBuilder sfcResponseBuilder, HermesContext hermesContext)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._context = hermesContext;
        this._context.Database.EnsureDeleted();
        this._context.Database.EnsureCreated();
        this._sut = new DefectRepository(_context);
    }

    [Fact]
    public async void GetConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefectList()
    {
        const int qty = 3;
        var sfcResponseBuilderWithDefects = _sfcResponseBuilder
            .SetOkContent()
            .AddBadDefect();
        for (var i = 0; i < qty; i++)
        {
            _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        }

        await _context.SaveChangesAsync();

        _context.Stop.Add(new Stop()
        {
            Defects = _context.Defects.ToList(),
            IsRestored = false
        });

        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(qty);
        Assert.Equal(qty, result.Count);
    }

    [Fact]
    public async void GetConsecutiveSameDefects_NotConsecutiveDefects_ReturnsEmptyList()
    {
        var sfcResponseBuilder = _sfcResponseBuilder
            .SetOkContent();
        var sfcResponseBuilderWithDefects = _sfcResponseBuilder.Clone()
            .SetOkContent()
            .AddBadDefect();
        _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        _context.SfcResponses.Add(sfcResponseBuilder.Build());
        await _context.SaveChangesAsync();

        var result = await _sut.GetNotRestoredConsecutiveSameDefects(3);
        Assert.Empty(result);
    }

    [Fact]
    public async void GetSameDefectsWithin1Hour_WithSameDefectsWithin1Hour_ReturnsDefectList()
    {
        var sfcResponseBuilder = _sfcResponseBuilder
            .SetOkContent();
        var sfcResponseBuilderWithDefects = _sfcResponseBuilder
            .Clone()
            .SetOkContent()
            .AddBadDefect();
        const int qty = 5;
        var timeLapse = TimeSpan.FromMinutes(59);
        var step = timeLapse / qty;
        while (timeLapse.TotalMinutes > 0)
        {
            sfcResponseBuilder.CreatedAt(DateTime.Now - timeLapse);
            sfcResponseBuilderWithDefects.CreatedAt(DateTime.Now - timeLapse);
            _context.SfcResponses.Add(sfcResponseBuilder.Build());
            _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
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
            _context.SfcResponses.Add(_sfcResponseBuilder
                .Clone()
                .SetOkContent()
                .AddBadDefect()
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
            _context.SfcResponses.Add(_sfcResponseBuilder
                .Clone()
                .SetOkContent()
                .AddBadDefect()
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
            _context.SfcResponses.Add(_sfcResponseBuilder
                .SetOkContent()
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyNotRestoredDefectsWithin1Hour(qty);
        Assert.Empty(result);
    }
}