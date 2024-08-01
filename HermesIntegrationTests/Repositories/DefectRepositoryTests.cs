using Hermes.Builders;
using Hermes.Repositories;

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
    public async void GetConsecutiveSameDefects_WithConsecutiveDefects_ReturnsDefect()
    {
        var sfcResponseBuilderWithDefects = _sfcResponseBuilder
            .SetOkContent()
            .AddBadDefect();
        _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
        await _context.SaveChangesAsync();

        var result = await _sut.GetConsecutiveSameDefects(3);
        Assert.False(result.IsNull);
        await _context.DisposeAsync();
    }

    [Fact]
    public async void GetConsecutiveSameDefects_NotConsecutiveDefects_ReturnsDefectNull()
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

        var result = await _sut.GetConsecutiveSameDefects(3);
        Assert.True(result.IsNull);
    }

    [Fact]
    public async void GetSameDefectsWithin1Hour_WithSameDefectsWithin1Hour_ReturnsDefect()
    {
        const int qty = 5;
        var sfcResponseBuilder = _sfcResponseBuilder
            .SetOkContent();
        var sfcResponseBuilderWithDefects = _sfcResponseBuilder
            .SetOkContent()
            .AddBadDefect();
        var timeLapse = TimeSpan.FromHours(1);
        var step = TimeSpan.FromMinutes(Math.Round(60.0 / qty));
        while (timeLapse.TotalMinutes > 0)
        {
            sfcResponseBuilder.CreatedAt(DateTime.Now - timeLapse);
            sfcResponseBuilderWithDefects.CreatedAt(DateTime.Now - timeLapse);
            _context.SfcResponses.Add(sfcResponseBuilder.Build());
            _context.SfcResponses.Add(sfcResponseBuilderWithDefects.Build());
            timeLapse -= step;
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetSameDefectsWithin1Hour(qty);
        Assert.False(result.IsNull);
    }

    [Fact]
    public async void GetSameDefectsWithin1Hour_NotSameDefectsWithin1Hour_ReturnsDefectNull()
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

        var result = await _sut.GetSameDefectsWithin1Hour(qty);
        Assert.True(result.IsNull);
    }

    [Fact]
    public async void GetDefectsWithin1Hour_AnyDefectsWithin1Hour_ReturnsDefectNull()
    {
        const int qty = 10;
        for (int i = 0; i <= qty; i++)
        {
            _context.SfcResponses.Add(_sfcResponseBuilder
                .Clone()
                .SetOkContent()
                .AddBadDefect()
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyDefectsWithin1Hour(qty);
        Assert.False(result.IsNull);
    }

    [Fact]
    public async void GetDefectsWithin1Hour_NoDefectsWithin1Hour_ReturnsDefectNull()
    {
        _context.Defects.RemoveRange(_context.Defects);
        await _context.SaveChangesAsync();
        const int qty = 10;
        for (int i = 0; i <= qty; i++)
        {
            _context.SfcResponses.Add(_sfcResponseBuilder
                .SetOkContent()
                .CreatedAt(DateTime.Now)
                .Build());
        }

        await _context.SaveChangesAsync();

        var result = await _sut.GetAnyDefectsWithin1Hour(qty);
        Assert.True(result.IsNull);
    }
}