using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using SukiUI.Dialogs;

namespace Hermes.Features.AdminTools;

public partial class FeaturePermissionsViewModel : ViewModelBase
{
    [ObservableProperty] private DepartmentType? _selectedDepartment;
    [ObservableProperty] private FeaturePermission? _selectedFeaturePermission;
    [ObservableProperty] private bool _isLoading;
    public ObservableCollection<FeaturePermission> FeaturePermissions { get; set; } = [];
    public static IEnumerable<DepartmentType?> DepartmentOptions => NullableExtensions.GetValues<DepartmentType>();

    private ManageFeaturePermissionDialogViewModel _manageFeatureDialogViewModel;
    private readonly FeaturePermissionRemoteRepository _featurePermissionRepository;
    private readonly ISukiDialogManager _dialogManager;

    public FeaturePermissionsViewModel(
        ISukiDialogManager dialogManager,
        FeaturePermissionRemoteRepository featurePermissionRepository)
    {
        this._dialogManager = dialogManager;
        this._featurePermissionRepository = featurePermissionRepository;
    }

    [RelayCommand]
    private async Task Search()
    {
        try
        {
            this.IsLoading = true;
            this.FeaturePermissions.Clear();
            this.FeaturePermissions
                .AddRange(await this._featurePermissionRepository
                    .GetAsync(this.SelectedDepartment));
        }
        catch (Exception e)
        {
            this.ShowErrorToast(e.Message);
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    private void Add()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog =>
            {
                this._manageFeatureDialogViewModel =
                    new ManageFeaturePermissionDialogViewModel(dialog, SelectedFeaturePermission);
                this._manageFeatureDialogViewModel.Accepted += (featurePermission) =>
                    Task.Run(() => Dispatcher.UIThread.InvokeAsync(() => this.Persist(featurePermission)));
                return this._manageFeatureDialogViewModel;
            })
            .TryShow();
    }

    private async Task Persist(FeaturePermission featurePermission)
    {
        try
        {
            this.IsLoading = true;
            await this._featurePermissionRepository.AddAndSaveAsync(featurePermission);
            this.ShowSuccessToast(Resources.msg_feature_permission_added);
            await this.Search();
        }
        catch (Exception e)
        {
            this.ShowErrorToast(e.Message);
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        try
        {
            this.IsLoading = true;
            if (SelectedFeaturePermission == null) return;
            await this._featurePermissionRepository.Delete(SelectedFeaturePermission);
            this.ShowSuccessToast(Resources.msg_feature_permission_deleted);
            await this.Search();
        }
        catch (Exception e)
        {
            this.ShowErrorToast(e.Message);
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    private void FeaturePermissionSelected(FeaturePermission? featurePermission)
    {
        this.SelectedFeaturePermission = featurePermission;
    }
}