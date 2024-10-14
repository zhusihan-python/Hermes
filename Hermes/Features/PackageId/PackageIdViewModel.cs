using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.PackageId;

public partial class PackageIdViewModel : PageBase
{
    [ObservableProperty] private bool _isLoadingPkgid;
    [ObservableProperty] private double _pkgidQtyUsed;
    [ObservableProperty] private double _pkgidPercentUsed;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SearchByPkgidCommand))]
    private string _pkgidText = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(PkgidReloadCommand))]
    private Package _package = Package.Null;

    public ObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];

    [ObservableProperty] private bool _isWorkOrderLoading;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SearchByWorkOrderCommand))]
    private string _workOrderText = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(WorkOrderReloadCommand))]
    private WorkOrder _workOrder = WorkOrder.Null;

    public ObservableCollection<Package> Packages { get; set; } = [];

    private readonly ISfcRepository _sfcRepository;

    public PackageIdViewModel(ISfcRepository sfcRepository)
        : base(
            Resources.txt_search_pkgid,
            MaterialIconKind.PackageVariant,
            PermissionType.FreeAccess,
            4,
            [StationType.Labeling, StationType.LabelingMachine])
    {
        this._sfcRepository = sfcRepository;
    }

    [RelayCommand(CanExecute = nameof(CanExecutePkgidKeyReload))]
    private async Task PkgidReload()
    {
        await this.SearchByPkgid(this.Package.Id);
    }

    private bool CanExecutePkgidKeyReload => !this.Package.IsNull;

    [RelayCommand]
    private async Task PkgidKeyDown()
    {
        await this.SearchByPkgid(this.PkgidText);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSearchByPkgid))]
    private async Task SearchByPkgid(string pkgid)
    {
        try
        {
            IsLoadingPkgid = true;
            this.UnitsUnderTest.Clear();

            var normalizedPkgId = Package.NormalizePkgId(pkgid);
            this.Package = await this._sfcRepository.FindPackage(normalizedPkgId);
            if (this.Package.IsNull)
            {
                this.ShowWarningToast(Resources.msg_package_not_found, Resources.txt_not_found);
            }
            else
            {
                var uuts = await this._sfcRepository.FindAllUnitsUnderTest(normalizedPkgId);
                this.UnitsUnderTest.AddRange(uuts.ToList());
                this.PkgidText = "";
                this.Package.Id = pkgid;
            }
        }
        catch (Exception)
        {
            this.ShowErrorToast(Resources.msg_error_while_getting_info_from_db);
            this.Package = Package.Null;
        }
        finally
        {
            this.PkgidQtyUsed = this.UnitsUnderTest.Count();
            this.PkgidPercentUsed = this.Package.Quantity > 0 ? this.PkgidQtyUsed / this.Package.Quantity * 100 : 0;
            IsLoadingPkgid = false;
        }
    }

    private bool CanExecuteSearchByPkgid => !string.IsNullOrEmpty(this.PkgidText);

    [RelayCommand(CanExecute = nameof(CanExecuteWorkOrderReload))]
    private async Task WorkOrderReload()
    {
        await this.SearchByWorkOrder(this.WorkOrder.Id);
    }

    private bool CanExecuteWorkOrderReload => !this.WorkOrder.IsNull;

    [RelayCommand]
    private async Task WorkOrderKeyDown()
    {
        await this.SearchByWorkOrder(this.WorkOrderText);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSearchByWorkOrder))]
    private async Task SearchByWorkOrder(string workOrder)
    {
        try
        {
            IsWorkOrderLoading = true;
            this.Packages.Clear();

            var normalizedWorkOrder = Package.NormalizeWorkOrder(workOrder);
            this.WorkOrder = await this._sfcRepository.FindWorkOrder(normalizedWorkOrder);
            if (this.WorkOrder.IsNull)
            {
                this.ShowWarningToast(Resources.msg_work_order_not_found, Resources.txt_not_found);
            }
            else
            {
                var packages = await this._sfcRepository.FindAllPackages(normalizedWorkOrder);
                this.Packages.AddRange(packages.ToList());
                this.WorkOrderText = "";
            }
        }
        catch (Exception)
        {
            this.WorkOrder = WorkOrder.Null;
        }
        finally
        {
            IsWorkOrderLoading = false;
        }
    }

    private bool CanExecuteSearchByWorkOrder => !string.IsNullOrEmpty(this.WorkOrderText);
}