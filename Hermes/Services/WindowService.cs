using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Features.SettingsConfig;
using Hermes.Features.UutProcessor;
using Hermes.Language;
using Hermes.Models;
using SukiUI.Toasts;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Runtime.Caching;

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
    private readonly Settings _settings;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly StopViewModel _stopViewModel;
    private readonly SuccessViewModel _successViewModel;
    private StopView? _stopView;
    private SuccessView? _successView;
    private SettingsView? _settingsView;
    private CancellationTokenSource _successViewCancellationTokenSource = new();
    private readonly ISukiToastManager _toastManager;
    private readonly MemoryCache _lastShownMessagesCache = MemoryCache.Default;
    private const ushort _maxShowCount = 4;
    private readonly TimeSpan _messageTTL = TimeSpan.FromSeconds(600);

    public WindowService(
        ViewLocator viewLocator,
        Settings settings,
        SettingsViewModel settingsViewModel,
        StopViewModel stopViewModel,
        SuccessViewModel successViewModel,
        ISukiToastManager toastManager)
    {
        this._viewLocator = viewLocator;
        this._settings = settings;
        this._successViewModel = successViewModel;
        this._stopViewModel = stopViewModel;
        this._stopViewModel.Restored += this.OnStopViewModelRestored;
        this._settingsViewModel = settingsViewModel;
        this._toastManager = toastManager;
    }

    public void Start()
    {
        Messenger.Register<ExitMessage>(this, this.Stop);
        Messenger.Register<ShowSettingsMessage>(this, this.ShowSettings);
        Messenger.Register<HideSettingsMessage>(this, this.HideSettings);
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
        this.SuccessView.ForceClose();
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
            await Task.Delay(this._settings.UutSuccessWindowTimeoutSeconds * 1000,
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

    public void ShowErrorToast(string message, string? title = null)
    {
        ShowToast(null, new ShowToastMessage(title ?? Resources.txt_error, message, NotificationType.Error));
    }

    public void ShowToast(object? recipient, ShowToastMessage message)
    {
        if (_lastShownMessagesCache.Contains(message.Value))
        {
            ushort showCount = (ushort)(_lastShownMessagesCache.Get(message.Value) ?? 0);
            if (showCount >= _maxShowCount)
            {
                // �ﵽ�����ʾ������������ʾ
                return;
            }

            else
            {
                showCount++;
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(_messageTTL)
                };
                _lastShownMessagesCache.Set(message.Value, showCount, policy);
            }
        }
        else
        {
            // ����Ϣ��ӵ����棬�����ù��ڲ���
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(_messageTTL)
            };
            _lastShownMessagesCache.Set(message.Value, (ushort)1, policy);
        }
        Dispatcher.UIThread.Invoke(() =>
        {
            _toastManager.CreateToast()
                .OfType(message.Type)
                .WithTitle(message.Title)
                .WithContent(message.Value)
                .Dismiss().After(TimeSpan.FromSeconds(message.Duration))
                .Dismiss().ByClicking()
                .Queue();
        });
    }

    private void ShowSettings(object recipient, ShowSettingsMessage message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            this._settingsViewModel.Refresh();
            this.SettingsView.Show();
        });
    }

    private void HideSettings(object recipient, HideSettingsMessage message)
    {
        Dispatcher.UIThread.Invoke(() => { this.SettingsView.Hide(); });
    }
}