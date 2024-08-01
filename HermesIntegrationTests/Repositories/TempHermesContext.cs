using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public sealed class TempHermesContext : HermesContext
{
    public TempHermesContext()
    {
        this.ConnectionString = "DataSource=file::memory:?cache=shared";
        try
        {
            Database.EnsureCreated();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}