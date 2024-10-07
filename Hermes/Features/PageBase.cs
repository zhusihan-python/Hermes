using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features;

public abstract partial class PageBase(
    string displayName,
    MaterialIconKind icon,
    PermissionType permissionType,
    int index = 0,
    List<StationType>? stationFilter = null)
    : ObservableRecipient
{
    [ObservableProperty] private string _displayName = displayName;
    [ObservableProperty] private MaterialIconKind _icon = icon;
    [ObservableProperty] private int _index = index;
    [ObservableProperty] private PermissionType _permissionType = permissionType;
    public List<StationType>? StationFilter { get; set; } = stationFilter;
}