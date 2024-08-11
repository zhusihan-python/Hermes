using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features;

public abstract partial class PageBase(string displayName, MaterialIconKind icon, PermissionLevel requiredViewPermissionLevel, int index = 0)
    : ObservableRecipient
{
    [ObservableProperty] private string _displayName = displayName;
    [ObservableProperty] private MaterialIconKind _icon = icon;
    [ObservableProperty] private int _index = index;
    [ObservableProperty] private PermissionLevel _requiredViewPermissionLevel = requiredViewPermissionLevel;
}