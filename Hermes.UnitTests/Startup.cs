using Hermes.Builders;
using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Services;
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