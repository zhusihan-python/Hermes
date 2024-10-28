using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class StopRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Stop, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task RestoreAsync(Stop stop)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        var entity = await ctx.Stops.FirstOrDefaultAsync(x => x.Id == stop.Id);
        if (entity == null) return;
        entity.IsRestored = true;
        await ctx.SaveChangesAsync();
    }
}