using Hermes.Builders;
using Hermes.Common;
using Hermes.Common.Parsers;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using HermesIntegrationTests.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HermesIntegrationTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextFactory<HermesLocalContext>(x =>
        {
            x.UseSqlite("DataSource=file::memory:?cache=shared");
        });
        services.AddTransient<AesEncryptor>();
        services.AddTransient<FileService>();
        services.AddTransient<ISettingsRepository, SettingsRepository>();
        services.AddTransient<ISfcRepository, SfcOracleRepository>();
        services.AddTransient<LabelingMachineUnitUnderTestParser>();
        services.AddTransient<ParserPrototype>();
        services.AddTransient<Settings>();
        services.AddTransient<SfcResponseBuilder>();
        services.AddTransient<UnitUnderTestBuilder>();
        services.AddTransient<GkgUnitUnderTestParser>();
        //services.AddTransient<DefectRepository>();
    }
}