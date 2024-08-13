using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.TokenGen.Features.TokenGen;

namespace Hermes.TokenGen.Features;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentPage = new TokenGenViewModel();
}