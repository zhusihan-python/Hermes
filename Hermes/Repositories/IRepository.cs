using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hermes.Repositories;

interface IRepository<T> where T : class
{
    public List<T> GetAll();
    public T? GetById(int id);
    public Task AddAsync(T entity);
    public void Edit(T entity);
    public void Delete(int id);
    public Task<int> SaveChangesAsync();
}