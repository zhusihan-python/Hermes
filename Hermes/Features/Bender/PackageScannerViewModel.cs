using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Repositories;
using SukiUI.Dialogs;

namespace Hermes.Features.Bender;

public partial class PackageScannerViewModel : ViewModelBase
{
    public event Action<Package>? PackageScanned;
    public event Action<string>? InstructionsChanged;

    [ObservableProperty] private Bitmap? _cover;
    [ObservableProperty] private Package _package = Package.Null;
    [ObservableProperty] private string _scannedCode = "";
    [ObservableProperty] private bool _isCodeGenerated;
    [ObservableProperty] private string _instructions = Resources.msg_change_wo;
    [ObservableProperty] private WorkOrder _workOrder = WorkOrder.Null;
    [ObservableProperty] private Bitmap? _partNumberImage;
    [ObservableProperty] private Bitmap? _revisionImage;
    [ObservableProperty] private Bitmap? _workOrderImage;

    private readonly PackageParser _packageParser;
    private readonly QrGenerator _qrGenerator;
    private readonly ISukiDialogManager _dialogManager;
    private readonly SfcOracleRepository _sfcOracleRepository;
    private readonly ISettingsRepository _settingsRepository;

    public PackageScannerViewModel(
        PackageParser packageParser,
        QrGenerator qrGenerator,
        ISukiDialogManager dialogManager,
        SfcOracleRepository oracleRepository,
        ISettingsRepository settingsRepository)
    {
        this._packageParser = packageParser;
        this._qrGenerator = qrGenerator;
        this._dialogManager = dialogManager;
        this._sfcOracleRepository = oracleRepository;
        this._settingsRepository = settingsRepository;
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
            this.Instructions = Resources.msg_scan_vendor;
            return;
        }

        this.Cover = await this._qrGenerator.GenerateAvaloniaBitmap(
            this.Package.ToString(), 150);
        this.IsCodeGenerated = true;
        await this.AddPackageToSfc();
        this.Instructions = Resources.msg_scan_2d_package;
    }

    private async Task AddPackageToSfc()
    {
        try
        {
            var package = await _sfcOracleRepository.FindPackageTracking(this.Package.NormalizedId);
            if (package.IsNull)
            {
                Package.Line = _settingsRepository.Settings.Line.ToUpperString();
                await _sfcOracleRepository.AddPackageTrack(Package);
            }
            else if (package.Line != _settingsRepository.Settings.Line.ToUpperString())
            {
                await _sfcOracleRepository.UpdatePackageTrackingLine(
                    package.NormalizedId,
                    _settingsRepository.Settings.Line.ToUpperString());
            }

            Messenger.Send(new ShowToastMessage("Success", "Package added to Hermes", NotificationType.Success));
            PackageScanned?.Invoke(Package);
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", e.Message, NotificationType.Error));
        }
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
        this.Instructions = Resources.msg_scan_2d_package;
    }

    partial void OnInstructionsChanged(string value)
    {
        Dispatcher.UIThread.Invoke(() => { this.InstructionsChanged?.Invoke(value); });
    }
}