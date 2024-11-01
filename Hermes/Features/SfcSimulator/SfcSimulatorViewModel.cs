using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using R3;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Hermes.Features.SfcSimulator;

public partial class SfcSimulatorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private SfcResponseType _mode = SfcResponseType.Ok;
    [ObservableProperty] private ErrorFlag _defectErrorFlag = ErrorFlag.Bad;
    [ObservableProperty] private string _defectLocation = "L1";
    [ObservableProperty] private string _defectErrorCode = "EC1";
    public IEnumerable<SfcResponseType> SfcErrorTypes => EnumExtensions.GetValues<SfcResponseType>();
    public IEnumerable<ErrorFlag> ErrorFlags => EnumExtensions.GetValues<ErrorFlag>();

    private readonly CoreSettings _coreSettings;
    private readonly FileService _fileService;
    private readonly ILogger _logger;
    private readonly SfcSimulatorService _sfcSimulatorService;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public SfcSimulatorViewModel(
        ILogger logger,
        CoreSettings coreSettings,
        FileService fileService,
        SfcSimulatorService sfcSimulatorService,
        UnitUnderTestBuilder underTestBuilder)
        : base(
            Resources.txt_sfc_simulator,
            MaterialIconKind.BugPlay,
            100)
    {
        _logger = logger;
        _coreSettings = coreSettings;
        _fileService = fileService;
        _sfcSimulatorService = sfcSimulatorService;
        _unitUnderTestBuilder = underTestBuilder;
        this.IsActive = true;
    }

    protected override void OnActivated()
    {
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        _sfcSimulatorService
            .IsRunning
            .Subscribe(x => IsRunning = x);
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
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    [RelayCommand]
    private void Stop()
    {
        try
        {
            _sfcSimulatorService.Stop();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
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
    private async Task CreatePassLogfileWithCustomDefect()
    {
        var builder = this._unitUnderTestBuilder
            .Clone()
            .IsPass(true)
            .AddDefect(new Defect()
            {
                ErrorFlag = this.DefectErrorFlag,
                Location = this.DefectLocation,
                ErrorCode = this.DefectErrorCode
            });
        await this.WriteLogfile(builder, "Pass");
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

    [RelayCommand]
    private void ShowPassView()
    {
        ShowSuccessView(isPass: true);
    }

    [RelayCommand]
    private void ShowRepairView()
    {
        ShowSuccessView(isPass: false);
    }

    [RelayCommand]
    private void ShowRepairViewWithMessage()
    {
        ShowSuccessView(isPass: false, Resources.msg_spi_repair);
    }

    private void ShowSuccessView(bool isPass, string message = "")
    {
        var uut = this._unitUnderTestBuilder.Clone()
            .IsPass(isPass)
            .Message(message)
            .IsSfcFail(false)
            .Build();
        Messenger.Send(new ShowSuccessMessage(uut));
    }

    [RelayCommand]
    private void ShowStopMachineView()
    {
        ShowStopView(StopType.Machine);
    }

    [RelayCommand]
    private void ShowStopLineView()
    {
        ShowStopView(StopType.Line);
    }

    private void ShowStopView(StopType type)
    {
        var stop = new Stop()
        {
            IsFake = true,
            SerialNumber = this._unitUnderTestBuilder.Build().SerialNumber,
            Type = type
        };
        Messenger.Send(new ShowStopMessage(stop));
    }


    private async Task WriteLogfile(UnitUnderTestBuilder builder, string fileNameWithoutExtension)
    {
        var uuid = Guid.NewGuid().ToString()[..5];
        var content = builder
            .SerialNumber($"1A62TEST{uuid}".ToUpper())
            .GetTestContent();
        await this._fileService.WriteAllTextToInputPathAsync($"{fileNameWithoutExtension}_{uuid}".ToUpper(), content);
    }

    partial void OnModeChanged(SfcResponseType value)
    {
        _sfcSimulatorService.Mode = value;
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }
}