using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.SimpleRouter;
using Hermes.Cipher;
using Hermes.TokenGen.Models;
using Hermes.TokenGen.Repositories;
using Hermes.TokenGen.Services;
using Hermes.TokenGen.ViewModels;
using Hermes.TokenGen.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.TokenGen;

public class App : Application
{
    public static bool IsAndroid => OperatingSystem.IsAndroid();
    public static bool IsDesktop => !IsAndroid;
    public static ToastService? ToastService { get; set; }
    public static IClipboard? Clipboard { get; set; }

    public static string AppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hermes.TokenGen"
    );

    public static string ConfigFullpath => Path.Combine(
        AppDataPath,
        "config.json"
    );

    public static string SubUsersFullpath => Path.Combine(
        AppDataPath,
        "users.json"
    );

    public static IStorageProvider? StorageProvider { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IServiceProvider services = ConfigureServices();
        var mainViewModel = services.GetRequiredService<MainViewModel>();
        ToastService = services.GetRequiredService<ToastService>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            StorageProvider = desktop.MainWindow.StorageProvider;
            Clipboard = desktop.MainWindow.Clipboard;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
            StorageProvider = TopLevel.GetTopLevel(singleViewPlatform.MainView)?.StorageProvider;
            Clipboard = TopLevel.GetTopLevel(singleViewPlatform.MainView)?.Clipboard;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<HistoryRouter<ViewModelBase>>(s =>
            new HistoryRouter<ViewModelBase>(t => (ViewModelBase)s.GetRequiredService(t)));

        services.AddSingleton<ToastService>(s =>
            OperatingSystem.IsAndroid() ? new AndroidToastService() : new DesktopToastService());

        services.AddSingleton<Session>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<TokenGenViewModel>();

        services.AddTransient<UserRepository>();

        services.AddSingleton<TokenGenerator>();
        return services.BuildServiceProvider();
    }
}