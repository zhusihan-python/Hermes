using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia;
using ConfigFactory.Avalonia.Helpers;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Features;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hermes
{
    public partial class App : Application
    {
        private readonly IServiceProvider? _provider;
        private WindowService? _windowService;
        private readonly ILogger? _logger;

        public App()
        {
            this._provider = this.ConfigureServices();
            this._logger = this._provider?.GetService<ILogger>()!;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Dispatcher.UIThread.UnhandledException += this.OnUnhandledException;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _provider?.GetService<ISettingsRepository>()?.Load();
                var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
                var mainViewModel = _provider?.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = viewLocator?.Build(mainViewModel) as Window;
                this._windowService = this._provider!.GetService<WindowService>();
                this._windowService!.MainView = desktop.MainWindow;
                this._windowService?.Start();
                this._provider?.GetRequiredService<HermesContext>().Migrate();
                if (mainViewModel != null) mainViewModel.IsActive = true;
                BrowserDialog.StorageProvider = desktop.MainWindow?.StorageProvider;
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            const string title = "Unhandled Exception";
            this._logger?.Error($"{title}: {e.Exception.Message}");
            this._windowService?.ShowToast(this, new ShowToastMessage(title, e.Exception.Message));
            e.Handled = true;
        }
    }
}