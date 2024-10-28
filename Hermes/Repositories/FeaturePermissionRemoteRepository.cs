using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class FeaturePermissionRemoteRepository(IDbContextFactory<HermesRemoteContext> context)
    : BaseRepository<FeaturePermission, HermesRemoteContext>(context)
{
    private readonly IDbContextFactory<HermesRemoteContext> _context = context;

    public async Task<IEnumerable<FeaturePermission>> FindAll(User user)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.FeaturePermissions
            .Where(x => x.Department <= user.Department && x.Level <= user.Level).ToListAsync();
    }
}