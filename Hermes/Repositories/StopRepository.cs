using System.Collections.Generic;
using System.Linq;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class StopRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Stop, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task RestoreAsync(Stop stop, List<User> users)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        var dbStop = await ctx.Stops.Where(x => x.Id == stop.Id).FirstOrDefaultAsync();
        if (dbStop == null) return;
        var dbUsers = await ctx.Users.Where(x => users.Contains(x)).ToListAsync();
        dbStop.IsRestored = true;
        dbStop.Users = dbUsers;
        await ctx.SaveChangesAsync();
    }
}