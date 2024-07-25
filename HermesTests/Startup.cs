using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;
using Hermes.Utils.Parsers;
using Microsoft.Extensions.DependencyInjection;

namespace HermesTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<Settings>();
        services.AddTransient<FileService>();
        services.AddTransient<ParserPrototype>();
        services.AddTransient<UnitUnderTestBuilder>();
        services.AddTransient<SfcResponseBuilder>();
    }
}