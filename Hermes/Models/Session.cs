using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Types;

namespace Hermes.Models;

public partial class Session : ObservableObject
{
    [ObservableProperty] private string _path = string.Empty;
    public Stop Stop { get; set; } = Stop.Null;

    public UutProcessorState UutProcessorState
    {
        get => _uutProcessorState;
        set
        {
            lock (_lock)
            {
                _uutProcessorState = value;
            }

            UutProcessorStateChanged?.Invoke(value);
        }
    }

    public event Action<UutProcessorState>? UutProcessorStateChanged;

    private readonly object _lock = new object();
    private UutProcessorState _uutProcessorState;

    public bool IsUutProcessorIdle => UutProcessorState == UutProcessorState.Idle;
    public bool IsUutProcessorBlocked => UutProcessorState == UutProcessorState.Blocked;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }
}