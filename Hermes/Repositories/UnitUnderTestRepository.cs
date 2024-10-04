using Hermes.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(HermesLocalContext db) : BaseRepository<UnitUnderTest, HermesLocalContext>(db)
{
    public async Task<List<UnitUnderTest>> GetLastUnitsUnderTest(int qty)
    {
        // Entity framework + Linq
        return await Db
            .UnitsUnderTest
            .OrderByDescending(x => x.Id)
            .Take(qty)
            .ToListAsync();
    }

    public async Task<List<UnitUnderTest>> Find(string sn, string msg)
    {
        return await Db
            .UnitsUnderTest
            .Where(x => x.SerialNumber == sn && x.Content == msg)
            .ToListAsync();
    }
}