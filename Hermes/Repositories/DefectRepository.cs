using Hermes.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Hermes.Repositories;

public class DefectRepository(HermesContext db) : BaseRepository<Defect>(db)
{
    public IEnumerable<Defect> GetAllLastHour()
    {
        return Db.Defects
            .Where(x => x.UnitUnderTest.CreatedAt > DateTime.Now.AddHours(-1))
            .OrderBy(x => x.UnitUnderTest.CreatedAt)
            .ToList();
    }

    public IQueryable<Defect> GetFromLastUnitsUnderTest(int qty)
    {
        var uuts = Db.UnitsUnderTest.Take(qty).OrderByDescending(x => x.CreatedAt);
        return Db.Defects
            .Where(x => uuts.Contains(x.UnitUnderTest));
    }
}