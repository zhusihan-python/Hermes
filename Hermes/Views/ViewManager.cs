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

namespace Hermes.Views;

public class ViewManager : ObservableRecipient
{
    private const int Width = 450;
    private const int Height = 130;

    private readonly Settings _settings;
    private readonly SuccessView _successView;
    private readonly SuccessViewModel _successViewModel;

    public ViewManager(
        Settings settings,
        SuccessView successView,
        SuccessViewModel successViewModel)
    {
        this._settings = settings;
        this._successViewModel = successViewModel;
        this._successView = successView;
    }

    public void Start()
    {
        Messenger.Register<ShowSuccessMessage>(this, this.ShowUutSuccess);
    }

    public void Stop()
    {
        Messenger.UnregisterAll(this);
        this._successView.Close();
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
        window.Width = Width;
        window.Height = Height;
        this._successView.UpdateLayout();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screen = desktop.MainWindow!.Screens.Primary;
            var screenSize = screen!.WorkingArea.Size;

            window.Position = new PixelPoint(
                screenSize.Width / 2 - Width / 2,
                screenSize.Height - Height - 5);
        }
    }
}