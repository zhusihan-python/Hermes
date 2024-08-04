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

namespace Hermes.Features.SfcSimulator;

public partial class SfcSimulatorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private SfcResponseType _mode = SfcResponseType.Ok;

    public IEnumerable<SfcResponseType> SfcErrorTypes =>
        Enum.GetValues(typeof(SfcResponseType)).Cast<SfcResponseType>();

    private readonly SfcSimulatorService _sfcSimulatorService;

    public SfcSimulatorViewModel(SfcSimulatorService sfcSimulatorService)
        : base("Sfc Simulator", MaterialIconKind.BugPlay, 0)
    {
        _sfcSimulatorService = sfcSimulatorService;

        _sfcSimulatorService.RunStatusChanged += OnRunStatusChange();
    }

    protected override void OnActivated()
    {
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    [RelayCommand]
    public void Start()
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