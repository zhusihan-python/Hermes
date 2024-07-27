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
    private readonly UutSenderService _uutSenderService;
    private readonly StopService _stopService;

    public UutProcessorViewModel(StopService stopService, UutSenderService uutSenderService)
        : base("UUT Processor", MaterialIconKind.FolderEye, 0)
    {
        this._stopService = stopService;
        this._uutSenderService = uutSenderService;
        this.IsActive = true;
    }

    protected override void OnActivated()
    {
        this._uutSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponseCreated += OnSfcResponseCreated;
        this._uutSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        this._uutSenderService.UnitUnderTestCreated -= OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponseCreated -= OnSfcResponseCreated;
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
        this.State = UutProcessorState.Processing;
        // TODO
    }

    private void OnSfcResponseCreated(object? sender, SfcResponse sfcResponse)
    {
        Task.Run((Func<Task?>)(async () =>
        {
            this.SerialNumber = string.Empty;
            this.State = UutProcessorState.Idle;
            var stop = await this._stopService.Calculate(sfcResponse);
            if (stop.IsNull)
            {
                Messenger.Send(new ShowSuccessMessage(sfcResponse));
            }
            else
            {
                Messenger.Send(new ShowStopMessage(stop));
            }
        }));
    }

    private void OnSfcSenderRunStatusChanged(object? sender, bool isRunning)
    {
        this.State = isRunning ? UutProcessorState.Idle : UutProcessorState.Stopped;
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
}