using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia;
using Hermes.Builders;
using Hermes.Common.Messages;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Features.Controls.Token;
using Hermes.Features.UutProcessor;
using Hermes.Features;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using Hermes.Common.Validators;

namespace Hermes
{
    public class App : Application
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
                var viewLocator = _provider?.GetRequiredService<IDataTemplate>();
                var mainViewModel = _provider?.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = viewLocator?.Build(mainViewModel) as Window;
                this._windowService = this._provider!.GetService<WindowService>();
                this._windowService!.MainView = desktop.MainWindow;
                this._windowService?.Start();
                this._provider?.GetRequiredService<HermesContext>().Migrate();
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

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Settings
            services.AddSingleton<Settings>();
            services.AddSingleton<Settings>();

            // Validators
            services.AddSingleton<MachineStopValidator>();
            services.AddSingleton<RuleThreeFiveTenValidator>();
            services.AddSingleton<ConsecutiveDefectsValidator>();
            services.AddSingleton<SameDefectsWithin1HourValidator>();

            // Repos
            services.AddSingleton<HermesContext>();
            services.AddTransient<SfcResponseRepository>();
            services.AddTransient<StopRepository>();
            services.AddTransient<UnitUnderTestRepository>();
            services.AddTransient<IDefectRepository, DefectRepository>();

            // Common
            services.AddSingleton<ILogger, HermesLogger>();
            services.AddSingleton<PageNavigationService>();
            services.AddSingleton<ParserPrototype>();
            services.AddSingleton<UnitUnderTestBuilder>();

            // Services
            services.AddSingleton<IDataTemplate, ViewLocator>();
            services.AddSingleton<PageNavigationService>();
            services.AddTransient<FileService>();
            services.AddTransient<FolderWatcherService>();
            services.AddTransient<UutSenderService>();
            services.AddTransient<SfcService>();
            services.AddTransient<StopService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<WindowService>();

            // Pages
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsAbstract && typeof(PageBase).IsAssignableFrom(p));
            foreach (var type in types)
            {
                services.AddSingleton(typeof(PageBase), type);
            }

            // Views
            services.AddTransient<StopView>();
            services.AddTransient<StopViewModel>();
            services.AddTransient<SuccessView>();
            services.AddTransient<SuccessViewModel>();
            services.AddTransient<TokenView>();
            services.AddTransient<TokenViewModel>();

            return services.BuildServiceProvider();
        }
    }
}