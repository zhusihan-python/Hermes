using CommunityToolkit.Mvvm.ComponentModel;

namespace Hermes.ViewModels;

public class ViewModelBase : ObservableRecipient
{
    public ViewModelBase()
    {
        this.IsActive = true;
    }
}