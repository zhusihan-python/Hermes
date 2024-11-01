using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Models;
using Hermes.Types;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System;

namespace Hermes.Features.AdminTools;

public partial class ManageFeaturePermissionDialogViewModel : ViewModelBase
{
    public event Action<FeaturePermission>? Accepted;

    [ObservableProperty] private DepartmentType _departmentSelected;
    [ObservableProperty] private PermissionType _permissionSelected;
    [ObservableProperty] private UserLevel _userLevelSelected;
    private readonly ISukiDialog _dialog;
    public DepartmentType[] Departments => Enum.GetValues<DepartmentType>();
    public static IEnumerable<PermissionType> PermissionOptions => EnumExtensions.GetValues<PermissionType>();
    public static IEnumerable<UserLevel> UserLevelOptions => EnumExtensions.GetValues<UserLevel>();

    public ManageFeaturePermissionDialogViewModel(ISukiDialog dialog, FeaturePermission? featurePermission)
    {
        this._dialog = dialog;
        this.DepartmentSelected = featurePermission?.Department ?? DepartmentType.All;
        this.PermissionSelected = featurePermission?.Permission ?? PermissionType.FreeAccess;
        this.UserLevelSelected = featurePermission?.Level ?? UserLevel.Operator;
    }

    [RelayCommand]
    private void Accept()
    {
        this.Accepted?.Invoke(new FeaturePermission()
        {
            Department = this.DepartmentSelected,
            Permission = this.PermissionSelected,
            Level = this.UserLevelSelected
        });
        this.CloseDialog();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Dispatcher.UIThread.InvokeAsync(() => _dialog.Dismiss());
    }
}