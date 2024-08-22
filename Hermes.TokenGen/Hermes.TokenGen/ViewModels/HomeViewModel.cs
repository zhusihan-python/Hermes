using Avalonia.SimpleRouter;
using CommunityToolkit.Mvvm.Input;
using Hermes.TokenGen.Models;
using Hermes.TokenGen.Repositories;

namespace Hermes.TokenGen.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly UserRepository _userRepository;
    private readonly Session _session;
    private readonly HistoryRouter<ViewModelBase> _router;

    public HomeViewModel(
        Session session,
        UserRepository userRepository,
        HistoryRouter<ViewModelBase> router)
    {
        _session = session;
        _userRepository = userRepository;
        _router = router;
    }

    [RelayCommand]
    private void NextPage()
    {
        _session.User = _userRepository.GetUser();
        if (_session.IsUserLoggedIn)
        {
            _router.GoTo<TokenGenViewModel>();
        }
        else
        {
            _router.GoTo<RegisterViewModel>();
        }
    }
}