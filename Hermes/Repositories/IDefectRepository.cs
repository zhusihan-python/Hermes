using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Models;

namespace Hermes.Repositories;

public interface IDefectRepository
{
    Task<List<Defect>> GetNotRestoredConsecutiveSameDefects(int qty);
    Task<List<Defect>> GetNotRestoredSameDefectsWithin1Hour(int qty);
    Task<List<Defect>> GetAnyNotRestoredDefectsWithin1Hour(int qty);
}