using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.TokenGen.Common.Messages;

namespace Hermes.TokenGen.Services;

public abstract class ToastService : ObservableRecipient
{
    protected ToastService()
    {
        IsActive = true;
    }

    protected override void OnActivated()
    {
        Messenger.Register<ShowToastMessage>(this, this.OnShowToast);
        base.OnActivated();
    }

    private void OnShowToast(object recipient, ShowToastMessage message)
    {
        ShowToast(message.Title, message.Value);
    }

    protected abstract void ShowToast(string title, string message);

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        base.OnDeactivated();
    }
}