using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Aspects;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private string _stateText = "";
    [ObservableProperty] private bool _isWaitingForDummy;
    private readonly Session _session;
    private readonly StopService _stopService;
    private readonly UutSenderServiceFactory _uutSenderServiceFactory;
    private IUutSenderService? _uutSenderService;
    public ScannerViewModel ScannerViewModel { get; }
    public DummyViewModel DummyViewModel { get; }

    public UutProcessorViewModel(
        Session session,
        StopService stopService,
        UutSenderServiceFactory uutSenderServiceFactory,
        ScannerViewModel scannerViewModel,
        DummyViewModel dummyViewModel)
        : base(
            Resources.txt_uut_processor,
            MaterialIconKind.FolderEye,
            PermissionType.OpenUutProcessor,
            1)
    {
        this._session = session;
        this._stopService = stopService;
        this._uutSenderServiceFactory = uutSenderServiceFactory;
        this.ScannerViewModel = scannerViewModel;
        this.DummyViewModel = dummyViewModel;
        this.IsActive = true;
        this.OnUutProcessorStateChanged(UutProcessorState.Stopped);
        this.StationFilter = EnumExtensions.GetValues<StationType>()
            .Where(x => x != StationType.Labeling && x != StationType.None)
            .ToList();
    }

    protected override void OnActivated()
    {
        this._session.UutProcessorStateChanged += OnUutProcessorStateChanged;
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<UnblockMessage>(this, this.OnUnblockReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        Messenger.Register<WaitForDummyMessage>(this, this.OnWaitForDummyMessage);
        base.OnActivated();
    }

    private void OnWaitForDummyMessage(object recipient, WaitForDummyMessage message)
    {
        if (this._uutSenderService != null)
        {
            this._uutSenderService.IsWaitingForDummy = message.Value;
        }
    }

    protected override void OnDeactivated()
    {
        if (_uutSenderService != null)
        {
            this._uutSenderService.UnitUnderTestCreated -= OnUnitUnderTestCreated;
            this._uutSenderService.SfcResponse -= OnSfcResponse;
            this._uutSenderService.RunStatusChanged -= OnSfcSenderRunStatusChanged;
        }

        Messenger.UnregisterAll(this);
    }

    [RelayCommand]
    [CatchExceptionAndShowErrorToast]
    private void Start()
    {
        try
        {
            if (this.IsRunning) return;
            this._uutSenderService = this.BuildUutSenderService();
            this._uutSenderService.Start();
            this.Path = this._uutSenderService.Path;
            this._stopService.Start();
            this.IsRunning = true;
            this.ShowInfoToast(Resources.msg_uut_processor_started);
        }
        catch (Exception)
        {
            this.Stop();
            throw;
        }
    }

    private IUutSenderService BuildUutSenderService()
    {
        var uutSenderService = this._uutSenderServiceFactory.Build();
        uutSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        uutSenderService.SfcResponse += OnSfcResponse;
        uutSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
        return uutSenderService;
    }

    [RelayCommand]
    [CatchExceptionAndShowErrorToast]
    private void Stop()
    {
        if (!this.IsRunning) return;
        this.IsRunning = false;
        this.SerialNumber = string.Empty;
        this.Path = this._uutSenderService?.Path ?? "";
        this._uutSenderService?.Stop();
        this._stopService.Stop();
        this.ShowInfoToast(Resources.msg_uut_processor_stopped);
    }

    private void OnUnitUnderTestCreated(object? sender, UnitUnderTest unitUnderTest)
    {
        this.SerialNumber = unitUnderTest.SerialNumber;
        this._session.UutProcessorState = UutProcessorState.Processing;
    }

    private void OnSfcResponse(object? sender, UnitUnderTest unitUnderTest)
    {
        Task.Run(async () =>
        {
            var stop = await this._stopService.Calculate(unitUnderTest);
            if (stop.IsNull)
            {
                Messenger.Send(new ShowSuccessMessage(unitUnderTest));
            }
            else
            {
                _session.Stop = stop;
                this._session.UutProcessorState = UutProcessorState.Blocked;
                Messenger.Send(new ShowStopMessage(stop));
                await this.WaitUntilUnblocked();
                _session.ResetStop();
            }

            this.SerialNumber = string.Empty;
            this._session.UutProcessorState = UutProcessorState.Idle;
        });
    }

    private async Task WaitUntilUnblocked()
    {
        while (this._session.IsUutProcessorBlocked)
        {
            await Task.Delay(100);
        }
    }

    private void OnSfcSenderRunStatusChanged(object? sender, bool isRunning)
    {
        this._session.UutProcessorState = isRunning ? UutProcessorState.Idle : UutProcessorState.Stopped;
        if (!isRunning)
        {
            this.Stop();
        }
    }

    private void OnStartReceive(object recipient, StartUutProcessorMessage message)
    {
        this.Start();
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }

    private void OnUutProcessorStateChanged(UutProcessorState uutProcessorState)
    {
        this.StateText = uutProcessorState.ToTranslatedString();
    }


    private void OnUnblockReceive(object recipient, UnblockMessage message)
    {
        this._session.UutProcessorState = UutProcessorState.Idle;
    }
}