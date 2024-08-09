using Hermes.Builders;
using Hermes.Common;
using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HermesTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<AesEncryptor>();
        services.AddTransient<FileService>();
        services.AddTransient<ISettingsRepository, SettingsRepository>();
        services.AddTransient<ParserPrototype>();
        services.AddTransient<Settings>();
        services.AddTransient<SfcResponseBuilder>();
        services.AddTransient<UnitUnderTestBuilder>();
    }
}