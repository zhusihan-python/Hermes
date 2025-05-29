using Hermes.Builders;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HermesTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(new Mock<ISfcRepository>().Object);
        services.AddTransient<AesEncryptor>();
        services.AddTransient<FileService>();
        services.AddTransient<ISettingsRepository, SettingsRepository>();
        services.AddTransient<LabelingMachineUnitUnderTestParser>();
        services.AddTransient<ParserPrototype>();
        services.AddTransient<Settings>();
        services.AddTransient<GkgUnitUnderTestParser>();
        services.AddTransient<FolderWatcherService>();
    }
}