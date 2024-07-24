using Hermes.Models;

namespace Hermes.Repositories;

public class UnitUnderTestRepository(HermesContext db) : BaseRepository<UnitUnderTest>(db);