using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Features.SettingsConfig;
using Hermes.Features.UutProcessor;
using Hermes.Repositories;
using SukiUI.Controls;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public class WindowService : ObservableRecipient
{
    private const int SuccessViewWidth = 450;
    private const int SuccessViewHeight = 130;

    private SettingsView SettingsView
    {
        get
        {
            if (_settingsView == null)
            {
                _settingsView = (_viewLocator.BuildWindow(_settingsViewModel) as SettingsView)!;
                _settingsView.Append(_settingsViewModel.Model);
            }

            return _settingsView;
        }
    }

    private SuccessView SuccessView => _successView ??= (_viewLocator.BuildWindow(_successViewModel) as SuccessView)!;
    private StopView StopView => _stopView ??= (_viewLocator.BuildWindow(_stopViewModel) as StopView)!;

    private readonly ViewLocator _viewLocator;
    private readonly ISettingsRepository _settingsRepository;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly StopViewModel _stopViewModel;
    private readonly SuccessViewModel _successViewModel;
    private StopView? _stopView;
    private SuccessView? _successView;
    private SettingsView? _settingsView;
    private CancellationTokenSource _successViewCancellationTokenSource = new();

    public WindowService(
        ViewLocator viewLocator,
        ISettingsRepository settingsRepository,
        SettingsViewModel settingsViewModel,
        StopViewModel stopViewModel,
        SuccessViewModel successViewModel)
    {
        this._viewLocator = viewLocator;
        this._settingsRepository = settingsRepository;
        this._successViewModel = successViewModel;
        this._stopViewModel = stopViewModel;
        this._stopViewModel.Restored += this.OnStopViewModelRestored;
        this._settingsViewModel = settingsViewModel;
    }

    public void Start()
    {
        Messenger.Register<ExitMessage>(this, this.Stop);
        Messenger.Register<ShowSettingsMessage>(this, this.ShowSettings);
        Messenger.Register<ShowStopMessage>(this, this.ShowStop);
        Messenger.Register<ShowSuccessMessage>(this, this.ShowUutSuccess);
        Messenger.Register<ShowToastMessage>(this, this.ShowToast);
    }

    private void Stop(object recipient, ExitMessage message)
    {
        this.Stop();
    }

    public void Stop()
    {
        Messenger.UnregisterAll(this);
        this.SuccessView.Close();
        this.StopView.ForceClose();
        this.SettingsView.ForceClose();
    }

    private void ShowUutSuccess(object recipient, ShowSuccessMessage message)
    {
        _successViewCancellationTokenSource.Cancel();
        _successViewCancellationTokenSource = new CancellationTokenSource();
        Dispatcher.UIThread.InvokeAsync(async Task () =>
        {
            SuccessView.DataContext = this._successViewModel;
            this._successViewModel.SerialNumber = message.Value.SerialNumber;
            this._successViewModel.IsRepair = message.Value.IsRepair;
            this._successViewModel.Message = message.Value.Message;

            SetBottomCenterPosition(SuccessView);
            SuccessView.UpdateLayout();
            SuccessView.Show();
            await Task.Delay(this._settingsRepository.Settings.UutSuccessWindowTimeoutSeconds * 1000,
                _successViewCancellationTokenSource.Token);
            if (!_successViewCancellationTokenSource.Token.IsCancellationRequested)
            {
                SuccessView.Hide();
            }
        });
    }

    private static void SetBottomCenterPosition(Window window)
    {
        window.Width = SuccessViewWidth;
        window.Height = SuccessViewHeight;
        window.UpdateLayout();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screen = desktop.MainWindow!.Screens.Primary;
            var screenSize = screen!.WorkingArea.Size;

            window.Position = new PixelPoint(
                screenSize.Width / 2 - SuccessViewWidth / 2,
                screenSize.Height - SuccessViewHeight - 5);
        }
    }

    private void ShowStop(object recipient, ShowStopMessage message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            this.StopView.DataContext = this._stopViewModel;
            this._stopViewModel.Reset();
            this._stopViewModel.UpdateDate();
            this._stopViewModel.Stop = message.Value;

            this.StopView.Show();
        });
    }

    private void OnStopViewModelRestored(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { this.StopView.Hide(); });
        Messenger.Send(new UnblockMessage());
    }

    public void ShowToast(object recipient, ShowToastMessage message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            SukiHost.ShowToast(message.Title, message.Value, duration: TimeSpan.FromSeconds(message.Duration));
        });
    }

    private void ShowSettings(object recipient, ShowSettingsMessage message)
    {
        Dispatcher.UIThread.Invoke(() => { this.SettingsView.Show(); });
    }
}