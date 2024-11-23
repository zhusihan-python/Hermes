using Microsoft.EntityFrameworkCore;
using System;
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
        var entity = await ctx.Set<T>().FindAsync(id);
        if (entity != null)
        {
            ctx.Set<T>().Remove(entity);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task<List<T>> GetAll()
    {
        await using var ctx = await context.CreateDbContextAsync();
        return await ctx.Set<T>().ToListAsync();
    }

    public async Task<List<T>> GetAll(Func<IQueryable<T>, IQueryable<T>>? filter = null)
    {
        await using var ctx = await context.CreateDbContextAsync();
        IQueryable<T> query = ctx.Set<T>();

        if (filter != null)
        {
            query = filter(query);
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetById(int id)
    {
        await using var ctx = await context.CreateDbContextAsync();
        return await ctx.Set<T>().FindAsync(id);
    }
}