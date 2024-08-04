using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Avalonia;
using Hermes.Builders;
using Hermes.Models;

namespace Hermes.Features.SfcSimulator;

public partial class SfcSimulatorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private SfcResponseType _mode = SfcResponseType.Ok;
    [ObservableProperty] private ErrorFlag _defectErrorFlag = ErrorFlag.Bad;
    [ObservableProperty] private string _defectLocation = "L1";
    [ObservableProperty] private string _defectErrorCode = "EC1";

    public IEnumerable<SfcResponseType> SfcErrorTypes =>
        Enum.GetValues(typeof(SfcResponseType)).Cast<SfcResponseType>();

    public IEnumerable<ErrorFlag> ErrorFlags =>
        Enum.GetValues(typeof(ErrorFlag)).Cast<ErrorFlag>();

    private readonly FileService _fileService;
    private readonly SfcSimulatorService _sfcSimulatorService;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly CoreSettings _coreSettings;

    public SfcSimulatorViewModel(
        CoreSettings coreSettings,
        FileService fileService,
        SfcSimulatorService sfcSimulatorService,
        UnitUnderTestBuilder underTestBuilder)
        : base("Sfc Simulator", MaterialIconKind.BugPlay, 0)
    {
        _coreSettings = coreSettings;
        _fileService = fileService;
        _sfcSimulatorService = sfcSimulatorService;
        _sfcSimulatorService.RunStatusChanged += OnRunStatusChange();
        _unitUnderTestBuilder = underTestBuilder;
    }

    protected override void OnActivated()
    {
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    [RelayCommand]
    private void Start()
    {
        try
        {
            _sfcSimulatorService.Start();
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error in SFC simulator", e.Message));
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _sfcSimulatorService.Stop();
    }

    [RelayCommand]
    private async Task CreatePassLogfile()
    {
        var builder = this._unitUnderTestBuilder
            .Clone()
            .IsPass(true);
        await this.WriteLogfile(builder, "Pass");
    }

    [RelayCommand]
    private async Task CreateFailLogfile()
    {
        this.DefectLocation = "L1";
        this.DefectErrorCode = "EC1";
        this.DefectErrorFlag = ErrorFlag.Bad;
        await this.CreateFailLogfileWithCustomDefect();
    }

    [RelayCommand]
    private async Task CreateCriticalFailLogfile()
    {
        this.DefectLocation = _coreSettings.GetFirstCriticalDefectLocation();
        this.DefectErrorCode = "CRITICAL_EC1";
        this.DefectErrorFlag = ErrorFlag.Bad;
        await this.CreateFailLogfileWithCustomDefect();
    }

    [RelayCommand]
    private async Task CreateFailLogfileWithCustomDefect()
    {
        var builder = this._unitUnderTestBuilder
            .Clone()
            .IsPass(false)
            .AddDefect(new Defect()
            {
                ErrorFlag = this.DefectErrorFlag,
                Location = this.DefectLocation,
                ErrorCode = this.DefectErrorCode
            });
        await this.WriteLogfile(builder, "Fail");
    }

    private async Task WriteLogfile(UnitUnderTestBuilder builder, string fileNameWithoutExtension)
    {
        var uuid = Guid.NewGuid().ToString()[..5];
        var content = builder
            .SerialNumber($"1A62TEST{uuid}".ToUpper())
            .GetContent();
        await this._fileService.WriteAllTextToInputPathAsync($"{fileNameWithoutExtension}_{uuid}".ToUpper(), content);
    }

    partial void OnModeChanged(SfcResponseType value)
    {
        _sfcSimulatorService.Mode = value;
    }

    private EventHandler<bool>? OnRunStatusChange()
    {
        return (sender, isRunning) => { IsRunning = isRunning; };
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }
}