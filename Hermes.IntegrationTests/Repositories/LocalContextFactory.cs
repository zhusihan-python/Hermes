using Hermes.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HermesIntegrationTests.Repositories;

class LocalContextFactory(HermesLocalContext context) : IDbContextFactory<HermesLocalContext>
{
    public HermesLocalContext CreateDbContext()
    {
        return context;
    }
}