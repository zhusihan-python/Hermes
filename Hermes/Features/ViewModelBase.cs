using CommunityToolkit.Mvvm.ComponentModel;

namespace Hermes.Features;

public abstract class ViewModelBase : ObservableRecipient
{
    protected ViewModelBase()
    {
        this.IsActive = true;
    }
}