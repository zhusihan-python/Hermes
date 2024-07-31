using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public static class HermesContextFactory
{
    public static HermesContext Build()
    {
        var context = new HermesContext();
        context.Database.EnsureCreated();

        return context;
    }
}