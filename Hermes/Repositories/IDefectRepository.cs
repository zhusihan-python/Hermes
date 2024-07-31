using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Repositories;

public interface IDefectRepository
{
    Task<Defect> GetConsecutiveSameDefects(int lastUnitsUnderTest);
    Task AddAsync(Defect entity);
    void Delete(int id);
    void Edit(Defect entity);
    List<Defect> GetAll();
    Defect? GetById(int id);
    Task<int> SaveChangesAsync();
}