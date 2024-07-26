using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia;
using Hermes.Models.Messages;
using Hermes.Utils.Extensions;
using Hermes.Utils;
using Hermes.ViewModels;
using Hermes.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hermes
{
    public class App : Application
    {
        public new static App Current => ((App)Application.Current!);
        public IServiceProvider Services { get; }
        private ViewManager? _viewManager;
        private readonly ILogger? _logger;

        public App()
        {
            var collection = new ServiceCollection();
            collection.AddCommonServices();
            this.Services = collection.BuildServiceProvider();
            this._logger = this.Services.GetService<ILogger>()!;
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
                desktop.MainWindow = new MainView
                {
                    DataContext = this.Services.GetService<MainViewModel>()
                };
                this._viewManager = this.Services.GetService<ViewManager>();
                this._viewManager!.MainView = desktop.MainWindow;
                this._viewManager?.Start();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var message = $"Unhandled Exception: {e.Exception.Message}";
            this._logger?.Error(message);
            this._viewManager?.ShowSnackbar(this, new ShowSnackbarMessage(message));
            e.Handled = true;
        }
    }
}