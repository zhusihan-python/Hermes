using System;
using System.Threading;
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

    public UutProcessorViewModel(SfcSenderService sfcSenderService)
    {
        this._sfcSenderService = sfcSenderService;
        sfcSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        sfcSenderService.SfcResponseCreated += OnSfcResponseCreated;
        sfcSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
    }

    [RelayCommand]
    private void Start()
    {
        this._sfcSenderService.Start();
        this.IsRunning = true;
    }

    [RelayCommand]
    public void Stop()
    {
        _sfcSenderService.Stop();
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
        this.SerialNumber = string.Empty;
        this.State = UutProcessorState.Idle;
        // TODO calc stop
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Messenger.Send(new ShowSuccessMessage(sfcResponse));
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