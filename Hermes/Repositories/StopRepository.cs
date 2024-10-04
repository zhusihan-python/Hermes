using Hermes.Models;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class StopRepository(HermesLocalContext db) : BaseRepository<Stop, HermesLocalContext>(db)
{
    public async Task RestoreAsync(Stop stop)
    {
        stop.IsRestored = true;
        await this.SaveChangesAsync();
    }
}