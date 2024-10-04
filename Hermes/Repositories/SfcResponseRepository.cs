using Hermes.Models;

namespace Hermes.Repositories;

public class SfcResponseRepository(HermesLocalContext db) : BaseRepository<SfcResponse, HermesLocalContext>(db);