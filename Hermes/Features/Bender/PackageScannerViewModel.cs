using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using System.Threading.Tasks;
using Hermes.Repositories;
using SukiUI.Dialogs;

namespace Hermes.Features.Bender;

public partial class PackageScannerViewModel : ViewModelBase
{
    [ObservableProperty] private Bitmap? _cover;
    [ObservableProperty] private Package _package = Package.Null;
    [ObservableProperty] private string _scannedCode = "";
    [ObservableProperty] private bool _isCodeGenerated;
    [ObservableProperty] private WorkOrder _workOrder = WorkOrder.Null;
    [ObservableProperty] private Bitmap? _partNumberImage;
    [ObservableProperty] private Bitmap? _revisionImage;
    [ObservableProperty] private Bitmap? _workOrderImage;

    private readonly PackageParser _packageParser;
    private readonly QrGenerator _qrGenerator;
    private readonly ISukiDialogManager _dialogManager;
    private readonly SfcOracleRepository _sfcOracleRepository;

    public PackageScannerViewModel(
        PackageParser packageParser,
        QrGenerator qrGenerator,
        ISukiDialogManager dialogManager,
        SfcOracleRepository oracleRepository)
    {
        this._packageParser = packageParser;
        this._qrGenerator = qrGenerator;
        this._dialogManager = dialogManager;
        this._sfcOracleRepository = oracleRepository;
    }


    [RelayCommand]
    private async Task ParsePackage()
    {
        this.Package = this._packageParser.Parse(this.ScannedCode);
        this.ScannedCode = "";
        await this.GenerateCode();
    }

    [RelayCommand]
    private async Task GenerateCode()
    {
        if (!this.Package.IsValid)
        {
            this.Cover = null;
            this.IsCodeGenerated = false;
            return;
        }

        this.Cover = await this._qrGenerator.GenerateAvaloniaBitmap(
            this.Package.ToString(), 150);
        this.IsCodeGenerated = true;
    }

    [RelayCommand]
    private void ChangeWorkOrder()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog =>
            {
                var vm = new WorkOrderDialogViewModel(dialog, _sfcOracleRepository);
                vm.WorkOrderSelected += (workOrder) => Task.Run(() => this.SetWorkOrder(workOrder));
                return vm;
            })
            .TryShow();
    }

    private async Task SetWorkOrder(WorkOrder workOrder)
    {
        this.WorkOrder = workOrder;
        if (this.WorkOrder.IsNull)
        {
            return;
        }

        this.PartNumberImage = await this._qrGenerator.GenerateAvaloniaBitmap((this.WorkOrder.PartNumber), 50);
        this.RevisionImage = await this._qrGenerator.GenerateAvaloniaBitmap((this.WorkOrder.Revision), 50);
        this.WorkOrderImage = await this._qrGenerator.GenerateAvaloniaBitmap((this.WorkOrder.Id), 50);
    }
}