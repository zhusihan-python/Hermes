using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hermes.Repositories;

interface IRepository<T> where T : class
{
    public Task<List<T>> GetAll();
    public Task<T?> GetById(int id);
    public Task AddAndSaveAsync(T entity);
    public Task Delete(int id);
}