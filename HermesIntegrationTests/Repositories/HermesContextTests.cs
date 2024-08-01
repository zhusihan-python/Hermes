using Hermes.Builders;
using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public class HermesContextTests
{
    private readonly HermesContext _sut;

    public HermesContextTests()
    {
        this._sut = new HermesContext();
    }

    [Fact]
    public void Initialize_AlreadyInitialized_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
        {
            _sut.Migrate();
            _sut.Migrate();
        });
    }
}