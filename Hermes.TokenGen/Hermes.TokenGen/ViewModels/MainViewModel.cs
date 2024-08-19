using Avalonia.SimpleRouter;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI;

namespace Hermes.TokenGen.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? _currentPage;

    public MainViewModel(HistoryRouter<ViewModelBase> router)
    {
        this.IsActive = true;
        router.CurrentViewModelChanged += viewModel => CurrentPage = viewModel;
        var theme = SukiTheme.GetInstance();
        if (theme.ActiveBaseTheme == ThemeVariant.Dark)
        {
            theme.SwitchBaseTheme();
        }

        router.GoTo<HomeViewModel>();
    }
}