using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public static class HermesContextFactory
{
    public static HermesLocalContext Build()
    {
        var context = new HermesLocalContext();
        context.Database.EnsureCreated();

        return context;
    }
}