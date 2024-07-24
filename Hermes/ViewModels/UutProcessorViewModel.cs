using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Models.Messages;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;

namespace Hermes.ViewModels;

public partial class UutProcessorViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private UutProcessorState _state = UutProcessorState.Disconnected;
    private readonly SfcSenderService _sfcSenderService;
    private readonly StopService _stopService;

    public UutProcessorViewModel(SfcSenderService sfcSenderService, StopService stopService)
    {
        this._sfcSenderService = sfcSenderService;
        this._stopService = stopService;
        sfcSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        sfcSenderService.SfcResponseCreated += OnSfcResponseCreated;
        sfcSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
    }

    [RelayCommand]
    private void Start()
    {
        this._sfcSenderService.Start();
        this._stopService.Start();
        this.IsRunning = true;
    }

    [RelayCommand]
    public void Stop()
    {
        this._sfcSenderService.Stop();
        this._stopService.Stop();
        this.IsRunning = false;
    }

    private void OnUnitUnderTestCreated(object? sender, UnitUnderTest unitUnderTest)
    {
        this.SerialNumber = unitUnderTest.SerialNumber;
        this.State = UutProcessorState.Processing;
        // TODO
    }

    private void OnSfcResponseCreated(object? sender, SfcResponse sfcResponse)
    {
        Task.Run((async () =>
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
        this.State = isRunning ? UutProcessorState.Idle : UutProcessorState.Disconnected;
        if (!isRunning)
        {
            this.Stop();
        }
    }
}