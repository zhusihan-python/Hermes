using System;
using Hermes.Models;
using Hermes.Types;
using Reactive.Bindings;

namespace Hermes.Services;

public interface IUutSenderService
{
    public ReactivePropertySlim<UutProcessorState> State { get; }
    event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    event EventHandler<UnitUnderTest>? SfcResponse;
    event EventHandler<bool>? RunStatusChanged;
    string Path { get; }
    bool IsWaitingForDummy { get; set; }
    void Start();
    void Stop();
}