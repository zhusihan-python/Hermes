using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.TokenGen.Common.Messages;
using SukiUI;

namespace Hermes.TokenGen.ViewModels;

public partial class MainViewModel : ViewModelBase, IRecipient<NavigateMessage>
{
    [ObservableProperty] private ViewModelBase _currentPage = new HomeViewModel();

    public MainViewModel()
    {
        this.IsActive = true;
        var theme = SukiTheme.GetInstance();
        if (theme.ActiveBaseTheme == ThemeVariant.Dark)
        {
            theme.SwitchBaseTheme();
        }
    }

    public void Receive(NavigateMessage message)
    {
        this.CurrentPage = message.Value;
    }
}