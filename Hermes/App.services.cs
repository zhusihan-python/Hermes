using AesEncryptor = Hermes.Common.AesEncryptor;
using Hermes.Builders;
using Hermes.Cipher;
using Hermes.Common.Parsers;
using Hermes.Common.Validators;
using Hermes.Common;
using Hermes.Features.Bender;
using Hermes.Features.Controls.Token;
using Hermes.Features.Login;
using Hermes.Features.SettingsConfig;
using Hermes.Features.SfcSimulator;
using Hermes.Features.UutProcessor;
using Hermes.Features;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Linq;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Hermes;

public partial class App
{
    private ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureModels(services);
        ConfigureValidators(services);
        ConfigureRepos(services);
        ConfigureCommon(services);
        ConfigureServices(services);
        ConfigurePages(services);
        ConfigureFeatures(services);
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
        services.AddDbContextFactory<HermesRemoteContext>();
        services.AddSingleton<FeaturePermissionRemoteRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<SerialScanner>();
        services.AddSingleton<UserProxy>();
        services.AddSingleton<UserRemoteRepository>();
        services.AddTransient<HermesLocalContext>();
        services.AddTransient<HermesRemoteContext>();
        services.AddTransient<IDefectRepository, DefectRepository>();
        services.AddTransient<ISfcRepository, SfcOracleRepository>();
        services.AddTransient<SfcResponseRepository>();
        services.AddTransient<StopRepository>();
        services.AddTransient<UnitUnderTestRepository>();
    }

    private static void ConfigureCommon(ServiceCollection services)
    {
        services.AddSingleton<AesEncryptor>();
        services.AddSingleton<GkgUnitUnderTestParser>();
        services.AddSingleton<ILogger, HermesLogger>();
        services.AddSingleton<LabelingMachineUnitUnderTestParser>();
        services.AddSingleton<PackageParser>();
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ParserPrototype>();
        services.AddSingleton<QrGenerator>();
        services.AddSingleton<SettingsConfigModel>();
        services.AddSingleton<SfcResponseBuilder>();
        services.AddSingleton<TokenGenerator>();
        services.AddSingleton<UnitUnderTestBuilder>();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<ViewLocator>();
        services.AddTransient<FileService>();
        services.AddTransient<FolderWatcherService>();
        services.AddTransient<ISfcService, SharedFolderSfcService>();
        services.AddTransient<SfcSimulatorService>();
        services.AddTransient<StopService>();
        services.AddTransient<TriUutSenderService>();
        services.AddTransient<GkgUutSenderService>();
        services.AddTransient<UutSenderServiceFactory>();
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

    private static void ConfigureFeatures(ServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SfcSimulatorViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<PackageScannerViewModel>();
        services.AddTransient<PackageTrackingViewModel>();
        services.AddTransient<ScannerViewModel>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<StopView>();
        services.AddTransient<StopViewModel>();
        services.AddTransient<SuccessView>();
        services.AddTransient<SuccessViewModel>();
        services.AddTransient<TokenView>();
        services.AddTransient<TokenViewModel>();
    }
}