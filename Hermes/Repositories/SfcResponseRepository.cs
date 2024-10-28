using Hermes.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Repositories;

public class SfcResponseRepository(IDbContextFactory<HermesLocalContext> context)
    : BaseRepository<SfcResponse, HermesLocalContext>(context);