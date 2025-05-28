using System.Collections.Generic;
using System.Linq;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class DoctorRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<Doctor, HermesLocalContext>(context)
{
    private readonly IDbContextFactory<HermesLocalContext> _context = context;

    public async Task<IEnumerable<Doctor>> FindAll()
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await FindAllDoctorQuery(ctx).ToListAsync();
    }

    public async Task<Doctor> FindById(string searchDoctorId)
    {
        await using var ctx = await _context.CreateDbContextAsync();
        return await ctx.Doctors.FirstOrDefaultAsync(x => x.Id.ToString() == searchDoctorId) ?? Doctor.Null;
    }

    private IQueryable<Doctor> FindAllDoctorQuery(HermesLocalContext ctx)
    {
        return ctx.Doctors.OrderBy(x => x.Id);
    }
}