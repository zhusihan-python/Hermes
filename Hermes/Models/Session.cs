using System;
using Hermes.Types;

namespace Hermes.Models;

public class Session
{
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

            UutProcessorStateChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<UutProcessorState>? UutProcessorStateChanged;

    private readonly object _lock = new object();
    private UutProcessorState _uutProcessorState;

    public bool IsUutProcessorIdle => UutProcessorState == UutProcessorState.Idle;
    public bool IsUutProcessorBlocked => UutProcessorState == UutProcessorState.Blocked;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }
}