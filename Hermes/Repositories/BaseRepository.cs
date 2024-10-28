using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class BaseRepository<T, TDbContext>(IDbContextFactory<TDbContext> context) : IRepository<T>
    where T : class
    where TDbContext : DbContext
{
    public virtual async Task AddAndSaveAsync(T entity)
    {
        await using var ctx = await context.CreateDbContextAsync();
        await ctx.Set<T>().AddAsync(entity);
        await ctx.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        await using var ctx = await context.CreateDbContextAsync();
        var q = await GetById(id);
        if (q != null) ctx.Set<T>().Remove(q);
        await ctx.SaveChangesAsync();
    }

    public async Task<List<T>> GetAll()
    {
        await using var ctx = await context.CreateDbContextAsync();
        return ctx.Set<T>().Select(a => a).ToList();
    }

    public async Task<T?> GetById(int id)
    {
        await using var ctx = await context.CreateDbContextAsync();
        return ctx.Set<T>().Find(id);
    }
}