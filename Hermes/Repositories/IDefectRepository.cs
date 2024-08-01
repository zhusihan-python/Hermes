using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Repositories;

public interface IDefectRepository
{
    Task<Defect> GetConsecutiveSameDefects(int qty);
    Task<Defect> GetSameDefectsWithin1Hour(int qty);
    Task<Defect> GetAnyDefectsWithin1Hour(int qty);
    Task AddAndSaveAsync(Defect entity);
    void Delete(int id);
    void Edit(Defect entity);
    List<Defect> GetAll();
    Defect? GetById(int id);
    Task<int> SaveChangesAsync();
}