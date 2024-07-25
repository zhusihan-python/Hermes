using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia;
using Hermes.ViewModels;
using Hermes.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using Avalonia.Threading;
using Hermes.Utils;
using Hermes.Utils.Extensions;

namespace Hermes
{
    public partial class App : Application
    {
        public new static App Current => ((App)Application.Current!)!;
        public IServiceProvider Services { get; }
        private ViewManager? _viewManager;

        public App()
        {
            var collection = new ServiceCollection();
            collection.AddCommonServices();
            this.Services = collection.BuildServiceProvider();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Avalonia.Threading.Dispatcher.UIThread.UnhandledException += this.OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var message = "Unhandled Exception";
            message += $": {e.Exception.Message}";
            this.Services.GetService<HermesLogger>()!.Error(message);
            e.Handled = true;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainView
                {
                    DataContext = this.Services.GetService<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
            this._viewManager = this.Services.GetService<ViewManager>();
            this._viewManager?.Start();
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            this._viewManager?.Stop();
        }
    }
}