using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Models.Messages;
using Hermes.Models;
using Hermes.ViewModels;
using System.Threading.Tasks;
using Material.Styles.Controls;
using Material.Styles.Models;

namespace Hermes.Views;

public class ViewManager : ObservableRecipient
{
    private const int SuccessViewWidth = 450;
    private const int SuccessViewHeight = 130;

    public Window? MainView { get; set; }

    private readonly Settings _settings;
    private readonly SuccessView _successView;
    private readonly SuccessViewModel _successViewModel;
    private readonly StopView _stopView;
    private readonly StopViewModel _stopViewModel;

    public ViewManager(
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
        Messenger.Register<ShowSnackbarMessage>(this, this.ShowSnackbar);
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
            this._stopViewModel.Stop = message.Value;

            this._stopView.Show();
        });
    }

    private void OnStopViewModelRestored(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { this._stopView.Hide(); });
    }

    public void ShowSnackbar(object recipient, ShowSnackbarMessage message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var snackbarModel = new SnackbarModel(message.Value, TimeSpan.FromSeconds(10));
            SnackbarHost.Post(snackbarModel, null, DispatcherPriority.Normal);
        });
    }
}