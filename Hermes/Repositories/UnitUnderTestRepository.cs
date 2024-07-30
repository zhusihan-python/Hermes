using Hermes.Models;
using System.Collections;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(HermesContext db) : BaseRepository<UnitUnderTest>(db)
{
    public IQueryable<UnitUnderTest> GetLast(int qty)
    {
        return Db.UnitsUnderTest.Take(qty);
    }
}