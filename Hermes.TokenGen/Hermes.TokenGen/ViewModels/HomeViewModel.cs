using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.TokenGen.Common.Messages;
using Hermes.TokenGen.Repositories;

namespace Hermes.TokenGen.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly UserRepository _userRepository;

    public HomeViewModel()
    {
        _userRepository = new UserRepository();
    }

    [RelayCommand]
    private void NextPage()
    {
        var user = _userRepository.GetUser();
        Messenger.Send(user.IsNull
            ? new NavigateMessage(new RegisterViewModel())
            : new NavigateMessage(new TokenGenViewModel(user)));
    }
}