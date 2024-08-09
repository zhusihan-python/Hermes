using Avalonia.Controls.Templates;
using Hermes.Builders;
using Hermes.Common.Parsers;
using Hermes.Common.Validators;
using Hermes.Common;
using Hermes.Features.Controls.Token;
using Hermes.Features.SettingsConfig;
using Hermes.Features.SfcSimulator;
using Hermes.Features.UutProcessor;
using Hermes.Features;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;

namespace Hermes;

public partial class App
{
    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        ConfigureModels(services);
        ConfigureValidators(services);
        ConfigureRepos(services);
        ConfigureCommon(services);
        ConfigureServices(services);
        ConfigureViewModels(services);
        ConfigurePages(services);
        ConfigureViews(services);
        return services.BuildServiceProvider();
    }

    private static void ConfigureModels(ServiceCollection services)
    {
        services.AddSingleton<CoreSettings>();
        services.AddSingleton<Session>();
    }

    private static void ConfigureValidators(ServiceCollection services)
    {
        services.AddSingleton<AnyDefectsWithin1HourValidator>();
        services.AddSingleton<ConsecutiveDefectsValidator>();
        services.AddSingleton<CriticalLocationStopValidator>();
        services.AddSingleton<MachineStopValidator>();
        services.AddSingleton<RuleThreeFiveTenValidator>();
        services.AddSingleton<SameDefectsWithin1HourValidator>();
    }

    private static void ConfigureRepos(ServiceCollection services)
    {
        services.AddSingleton<HermesContext>();
        services.AddTransient<IDefectRepository, DefectRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddTransient<SfcResponseRepository>();
        services.AddTransient<StopRepository>();
        services.AddTransient<UnitUnderTestRepository>();
    }

    private static void ConfigureCommon(ServiceCollection services)
    {
        services.AddSingleton<AesEncryptor>();
        services.AddSingleton<ILogger, HermesLogger>();
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ParserPrototype>();
        services.AddSingleton<SettingsConfigModel>();
        services.AddSingleton<SfcResponseBuilder>();
        services.AddSingleton<UnitUnderTestBuilder>();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IDataTemplate, ViewLocator>();
        services.AddSingleton<PageNavigationService>();
        services.AddTransient<FileService>();
        services.AddTransient<FolderWatcherService>();
        services.AddTransient<ISfcService, SharedFolderSfcService>();
        services.AddTransient<SfcSimulatorService>();
        services.AddTransient<StopService>();
        services.AddTransient<UutSenderService>();
    }

    private static void ConfigureViewModels(ServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SfcSimulatorViewModel>();
        services.AddTransient<WindowService>();
    }

    private static void ConfigurePages(ServiceCollection services)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => !p.IsAbstract && typeof(PageBase).IsAssignableFrom(p));
        foreach (var type in types)
        {
            services.AddSingleton(typeof(PageBase), type);
        }
    }

    private static void ConfigureViews(ServiceCollection services)
    {
        services.AddTransient<SettingsView>();
        services.AddTransient<StopView>();
        services.AddTransient<StopViewModel>();
        services.AddTransient<SuccessView>();
        services.AddTransient<SuccessViewModel>();
        services.AddTransient<TokenView>();
        services.AddTransient<TokenViewModel>();
    }
}