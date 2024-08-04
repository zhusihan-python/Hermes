using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Features.UutProcessor;
using Hermes.Models;
using SukiUI.Controls;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class WindowService : ObservableRecipient
{
    private const int SuccessViewWidth = 450;
    private const int SuccessViewHeight = 130;

    public Window? MainView { get; set; }

    private readonly Settings _settings;
    private readonly SuccessView _successView;
    private readonly SuccessViewModel _successViewModel;
    private readonly StopView _stopView;
    private readonly StopViewModel _stopViewModel;

    public WindowService(
        Settings settings,
        SuccessView successView,
        SuccessViewModel successViewModel,
        StopView stopView,
        StopViewModel stopViewModel)
    {
        this._settings = settings;
        this._successViewModel = successViewModel;
        this._successView = successView;
        this._stopView = stopView;
        this._stopViewModel = stopViewModel;
        stopViewModel.Restored += this.OnStopViewModelRestored;
    }

    public void Start()
    {
        Messenger.Register<ShowSuccessMessage>(this, this.ShowUutSuccess);
        Messenger.Register<ShowStopMessage>(this, this.ShowStop);
        Messenger.Register<ShowToastMessage>(this, this.ShowToast);
        Messenger.Register<ExitMessage>(this, (_, __) => this.Stop());
    }

    public void Stop()
    {
        Messenger.UnregisterAll(this);
        this._successView.Close();
        this._stopView.CanClose = true;
        this._stopView.Close();
        this.MainView?.Close();
    }

    private void ShowUutSuccess(object recipient, ShowSuccessMessage message)
    {
        Dispatcher.UIThread.InvokeAsync(async Task () =>
        {
            this._successView.DataContext = this._successViewModel;
            this._successViewModel.SerialNumber = message.Value.SerialNumber;
            this._successViewModel.IsRepair = message.Value.IsRepair;

            this.SetBottomCenterPosition(this._successView);
            this._successView.Show();
            await Task.Delay(this._settings.UutSuccessWindowTimeoutSeconds * 1000);
            this._successView.Hide();
        });
    }

    private void SetBottomCenterPosition(Window window)
    {
        window.Width = SuccessViewWidth;
        window.Height = SuccessViewHeight;
        this._successView.UpdateLayout();
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
            this._stopView.DataContext = this._stopViewModel;
            this._stopViewModel.Reset();
            this._stopViewModel.UpdateDate();
            this._stopViewModel.Stop = message.Value;

            this._stopView.Show();
        });
    }

    private void OnStopViewModelRestored(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { this._stopView.Hide(); });
        Messenger.Send(new UnblockMessage());
    }

    public void ShowToast(object recipient, ShowToastMessage message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            SukiHost.ShowToast(message.Title, message.Value, duration: TimeSpan.FromSeconds(message.Duration));
        });
    }
}