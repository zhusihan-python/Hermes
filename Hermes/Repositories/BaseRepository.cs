using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class BaseRepository<T, TDbContext>(TDbContext db) : IRepository<T>
    where T : class
    where TDbContext : DbContext
{
    protected readonly TDbContext Db = db;

    public virtual async Task AddAndSaveAsync(T entity)
    {
        await this.Db.Set<T>().AddAsync(entity);
        await this.SaveChangesAsync();
    }

    public void Delete(int id)
    {
        var q = GetById(id);
        if (q != null) this.Db.Set<T>().Remove(q);
    }

    public void Edit(T entity)
    {
        this.Db.Entry<T>(entity).State = EntityState.Modified;
    }

    public List<T> GetAll()
    {
        return this.Db.Set<T>().Select(a => a).ToList();
    }

    public T? GetById(int id)
    {
        return this.Db.Set<T>().Find(id);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await this.Db.SaveChangesAsync();
    }
}