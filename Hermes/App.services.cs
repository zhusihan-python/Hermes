using AesEncryptor = Hermes.Common.AesEncryptor;
using Hermes.Builders;
using Hermes.Cipher;
using Hermes.Common.Parsers;
using Hermes.Common.Reactive;
using Hermes.Common.Validators;
using Hermes.Common;
using Hermes.Communication.SerialPort;
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

namespace Hermes;

public partial class App
{
    public ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureModels(services);
        ConfigureValidators(services);
        ConfigureRepos(services);
        ConfigureCommon(services);
        ConfigureServices(services);
        ConfigureFeatures(services);
        return services.BuildServiceProvider();
    }

    private static void ConfigureModels(ServiceCollection services)
    {
        services.AddSingleton<Session>();
        services.AddSingleton<ServiceProvider>();
        services.AddSingleton<Device>();
        services.AddSingleton<FrameParser>();
        services.AddSingleton<Settings>(sp => sp.GetRequiredService<ISettingsRepository>().Read());
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
        services.AddDbContextFactory<HermesLocalContext>();
        services.AddDbContextFactory<HermesRemoteContext>();
        services.AddSingleton<FeaturePermissionRemoteRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<SerialScanner>();
        services.AddSingleton<UserRemoteRepository>();
        services.AddSingleton<UserRepositoryProxy>();
        services.AddTransient<HermesRemoteContext>();
        //services.AddTransient<IDefectRepository, DefectRepository>();
        services.AddTransient<ISfcRepository, SfcOracleRepository>();
        services.AddTransient<SfcResponseRepository>();
        //services.AddTransient<StopRepository>();
        services.AddTransient<DoctorRepository>();
        services.AddTransient<SlideRepository>();
        //services.AddTransient<UnitUnderTestRepository>();
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
        services.AddSingleton<SfcResponseBuilder>();
        services.AddSingleton<TokenGenerator>();
        services.AddSingleton<ComPort>();
        services.AddSingleton<ScanEngine>();
        services.AddSingleton<MessageSender>();
        services.AddTransient<SerialPortRx>();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<PageNavigationService>();
        services.AddSingleton<PagePrototype>();
        services.AddSingleton<ViewLocator>();
        services.AddTransient<FileService>();
        services.AddTransient<FileSystemWatcherRx>();
        services.AddTransient<FolderWatcherService>();
        services.AddTransient<ISfcService, SharedFolderSfcService>();
        //services.AddTransient<ServiceProvider>();
        services.AddTransient<StopService>();
        services.AddTransient<WindowService>();
    }

    private static void ConfigureFeatures(ServiceCollection services)
    {
        Type[] singletonTypes =
            [typeof(MainWindowViewModel), typeof(UutProcessorViewModel)];
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var viewModelTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("ViewModel") && t is { IsClass: true, IsAbstract: false });

            foreach (var viewModelType in viewModelTypes)
            {
                if (singletonTypes.Contains(viewModelType) || viewModelType.IsSubclassOf(typeof(PageBase)))
                {
                    services.AddSingleton(viewModelType);
                }
                else
                {
                    services.AddScoped(viewModelType);
                }
            }
        }
    }
}