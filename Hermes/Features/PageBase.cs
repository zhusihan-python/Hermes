using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace Hermes.Features;

public abstract partial class PageBase(
    string displayName,
    MaterialIconKind icon,
    int index = 0)
    : ViewModelBase
{
    [ObservableProperty] private string _displayName = displayName;
    [ObservableProperty] private MaterialIconKind _icon = icon;
    [ObservableProperty] private int _index = index;
}