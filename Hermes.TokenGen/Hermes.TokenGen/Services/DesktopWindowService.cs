using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.TokenGen.Common.Messages;

namespace Hermes.TokenGen.Services;

public class DesktopWindowService : ObservableRecipient
{
    protected override void OnActivated()
    {
        Messenger.Register<OpenWindowMessage>(this, this.OnOpenWindow);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        base.OnDeactivated();
    }


    private void OnOpenWindow(object recipient, OpenWindowMessage message)
    {
        var locator = new ViewLocator();
        if (locator.Build(message.Value) is Window window)
        {
            window.DataContext = message.Value;
            window.Show();
        }
    }
}