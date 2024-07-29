using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{
    private readonly HermesContext _db;

    public BaseRepository(HermesContext db)
    {
        this._db = db;
    }

    public virtual async Task AddAsync(T entity)
    {
        await this._db.Set<T>().AddAsync(entity);
        await this.SaveChangesAsync();
    }

    public void Delete(int id)
    {
        var q = GetById(id);
        if (q != null) this._db.Set<T>().Remove(q);
    }

    public void Edit(T entity)
    {
        this._db.Entry<T>(entity).State = EntityState.Modified;
    }

    public List<T> GetAll()
    {
        return this._db.Set<T>().Select(a => a).ToList();
    }

    public T? GetById(int id)
    {
        return this._db.Set<T>().Find(id);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await this._db.SaveChangesAsync();
    }
}