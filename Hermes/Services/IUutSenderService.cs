using System;
using Hermes.Models;

namespace Hermes.Services;

public interface IUutSenderService
{
    event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    event EventHandler<UnitUnderTest>? SfcResponse;
    event EventHandler<bool>? RunStatusChanged;
    string Path { get; }
    void Start();
    void Stop();
}