using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Hermes.TokenGen.ViewModels;
using Hermes.TokenGen.Views;

namespace Hermes.TokenGen;

public partial class App : Application
{
    public static bool IsDesktop { get; set; }

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            IsDesktop = true;
            StorageProvider = desktop.MainWindow.StorageProvider;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
            StorageProvider = TopLevel.GetTopLevel(singleViewPlatform.MainView)?.StorageProvider;
        }

        base.OnFrameworkInitializationCompleted();
    }
}