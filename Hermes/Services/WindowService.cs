using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using SukiUI.Toasts;
using System.Threading;
using System;
using System.Runtime.Caching;

namespace Hermes.Services;

public class WindowService : ObservableRecipient
{
    private const int SuccessViewWidth = 450;
    private const int SuccessViewHeight = 130;

    private readonly ViewLocator _viewLocator;
    private CancellationTokenSource _successViewCancellationTokenSource = new();
    private readonly ISukiToastManager _toastManager;
    private readonly MemoryCache _lastShownMessagesCache = MemoryCache.Default;
    private const ushort _maxShowCount = 4;
    private readonly TimeSpan _messageTTL = TimeSpan.FromSeconds(600);

    public WindowService(
        ViewLocator viewLocator,
        Settings settings,
        ISukiToastManager toastManager)
    {
        this._viewLocator = viewLocator;
        this._toastManager = toastManager;
    }

    public void Start()
    {
        Messenger.Register<ExitMessage>(this, this.Stop);
        Messenger.Register<ShowToastMessage>(this, this.ShowToast);
    }

    private void Stop(object recipient, ExitMessage message)
    {
        this.Stop();
    }

    public void Stop()
    {
        Messenger.UnregisterAll(this);
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
                // 达到最大显示次数，不再显示
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
            // 将消息添加到缓存，并设置过期策略
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
}