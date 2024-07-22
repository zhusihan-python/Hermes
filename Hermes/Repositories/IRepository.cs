using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hermes.Repositories;

interface IRepository<T> where T : class
{
    List<T> GetAll();
    T GetById(int id);
    Task AddAsync(T entity);
    void Edit(T entity);
    void Delete(int id);
    Task<int> SaveChangesAsync();
}