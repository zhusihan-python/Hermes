using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Cipher.Types;
using Hermes.Types;

namespace Hermes.Repositories;

public class FeaturePermissionRemoteRepository(IDbContextFactory<HermesRemoteContext> context)
    : BaseRepository<FeaturePermission, HermesRemoteContext>(context)
{
    private readonly IDbContextFactory<HermesRemoteContext> _context = context;

    public async Task Delete(FeaturePermission featurePermission)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        await ctx.FeaturePermissions
            .Where(x => x.Department == featurePermission.Department && x.Permission == featurePermission.Permission &&
                        x.Level == featurePermission.Level)
            .ExecuteDeleteAsync();
        await ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<FeaturePermission>> GetAsync(
        DepartmentType? department,
        UserLevel? level = null)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        var query = ctx.FeaturePermissions.AsQueryable();
        if (department != null)
        {
            query = query.Where(x => x.Department == department || x.Department == DepartmentType.All);
        }

        if (level != null)
        {
            query = query.Where(x => x.Level == level);
        }

        return await query
            .OrderBy(x => x.Department)
            .ThenBy(x => x.Permission)
            .ThenBy(x => x.Level)
            .ToListAsync();
    }
}