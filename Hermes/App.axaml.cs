using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia;
using Hermes.ViewModels;
using Hermes.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = "Unhandled Exception";
            if (e.ExceptionObject is Exception exception)
            {
                message += $": {exception.Message}";
            }

            this.Services.GetService<HermesLogger>()!.Error(message);
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