using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class FeaturePermissionRemoteRepository(HermesRemoteContext db)
    : BaseRepository<FeaturePermission, HermesRemoteContext>(db)
{
    private readonly HermesRemoteContext _db = db;

    public async Task<IEnumerable<FeaturePermission>> FindAll(User user)
    {
        return await _db.FeaturePermissions
            .Where(x => x.Department <= user.Department && x.Level <= user.Level).ToListAsync();
    }
}