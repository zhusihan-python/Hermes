using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.SfcSimulator;

public partial class SfcSimulatorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private SfcResponseType _mode = SfcResponseType.Ok;
    public IEnumerable<SfcResponseType> SfcErrorTypes => Enum.GetValues(typeof(SfcResponseType)).Cast<SfcResponseType>();

    private readonly SfcSimulatorService _sfcSimulatorService;

    public SfcSimulatorViewModel(SfcSimulatorService sfcSimulatorService)
        : base("Sfc Simulator", MaterialIconKind.BugPlay, 1)
    {
        _sfcSimulatorService = sfcSimulatorService;

        _sfcSimulatorService.RunStatusChanged += OnRunStatusChange();
    }

    [RelayCommand]
    public void Start()
    {
        _sfcSimulatorService.Start();
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
}