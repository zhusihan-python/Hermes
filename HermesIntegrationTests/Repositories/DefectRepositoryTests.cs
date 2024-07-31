using Hermes.Builders;
using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public class DefectRepositoryTests
{
    private readonly DefectRepository _sut;
    private readonly HermesContext _context;
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public DefectRepositoryTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._context = HermesContextFactory.Build();
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
        await _context.DisposeAsync();
    }
}