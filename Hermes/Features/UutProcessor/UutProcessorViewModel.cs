using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private UutProcessorState _state = UutProcessorState.Stopped;
    private readonly Session _session;
    private readonly StopService _stopService;
    private readonly UutSenderService _uutSenderService;

    public UutProcessorViewModel(Session session, StopService stopService, UutSenderService uutSenderService)
        : base("UUT Processor", MaterialIconKind.FolderEye, 1)
    {
        this._session = session;
        this._stopService = stopService;
        this._uutSenderService = uutSenderService;
        this.IsActive = true;
    }

    protected override void OnActivated()
    {
        this._session.UutProcessorStateChanged += OnUutProcessorStateChanged;
        this._uutSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponse += OnSfcResponse;
        this._uutSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<UnblockMessage>(this, this.OnUnblockReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        this._uutSenderService.UnitUnderTestCreated -= OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponse -= OnSfcResponse;
        this._uutSenderService.RunStatusChanged -= OnSfcSenderRunStatusChanged;
        Messenger.UnregisterAll(this);
    }

    [RelayCommand]
    private void Start()
    {
        if (this.IsRunning) return;
        this._uutSenderService.Start();
        this._stopService.Start();
        this.IsRunning = true;
        Messenger.Send(new ShowToastMessage("Info", "UUT Processor started"));
    }

    [RelayCommand]
    public void Stop()
    {
        if (!this.IsRunning) return;
        this._uutSenderService.Stop();
        this._stopService.Stop();
        this.IsRunning = false;
        this.SerialNumber = string.Empty;
        Messenger.Send(new ShowToastMessage("Info", "UUT Processor stopped"));
    }

    private void OnUnitUnderTestCreated(object? sender, UnitUnderTest unitUnderTest)
    {
        this.SerialNumber = unitUnderTest.SerialNumber;
        this._session.UutProcessorState = UutProcessorState.Processing;
    }

    private void OnSfcResponse(object? sender, UnitUnderTest unitUnderTest)
    {
        Task.Run((Func<Task?>)(async () =>
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
        }));
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

    private void OnUutProcessorStateChanged(object? sender, UutProcessorState uutProcessorState)
    {
        this.State = uutProcessorState;
    }


    private void OnUnblockReceive(object recipient, UnblockMessage message)
    {
        this._session.UutProcessorState = UutProcessorState.Idle;
    }
}