using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Services;
using Hermes.TokenGen.Common.Messages;
using Hermes.TokenGen.Models;

namespace Hermes.TokenGen.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [RelayCommand]
    private void NextPage()
    {
        if (FileService.FileExists(App.ConfigFullpath))
        {
            try
            {
                var user = FileService.ReadJsonEncrypted<User>(App.ConfigFullpath);
                if (user is not null)
                {
                    Messenger.Send(new NavigateMessage(new TokenGenViewModel(user)));
                    return;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        Messenger.Send(new NavigateMessage(new RegisterViewModel()));
    }
}