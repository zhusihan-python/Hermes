using CommunityToolkit.Mvvm.ComponentModel;

namespace Hermes.ViewModels;

public abstract class ViewModelBase : ObservableRecipient
{
    protected ViewModelBase()
    {
        this.IsActive = true;
    }
}