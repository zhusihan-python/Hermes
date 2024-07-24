using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Models;

namespace Hermes.ViewModels;

public partial class SuccessViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isRepair;
    [ObservableProperty] private string _serialNumber = "";
}