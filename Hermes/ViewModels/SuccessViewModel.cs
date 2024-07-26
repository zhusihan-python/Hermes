using CommunityToolkit.Mvvm.ComponentModel;

namespace Hermes.ViewModels;

public partial class SuccessViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isRepair;
    [ObservableProperty] private string _serialNumber = "";
}