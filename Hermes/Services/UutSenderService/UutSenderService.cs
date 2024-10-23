using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using Hermes.Types;
using Reactive.Bindings;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System;

namespace Hermes.Services.UutSenderService;

public abstract class UutSenderService
{
    public ReactiveProperty<StateType> State { get; } = new(StateType.Stopped);
    public ReactiveProperty<UnitUnderTest> UnitUnderTestCreated { get; } = new(UnitUnderTest.Null);
    public ReactiveProperty<SfcResponse> SfcResponseCreated { get; } = new(SfcResponse.Null);
    public ReactiveProperty<bool> IsRunning { get; } = new(false);

    public abstract string Path { get; }
    public bool IsWaitingForDummy { get; set; }

    private readonly ILogger _logger;
    private readonly ISfcService _sfcService;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    protected readonly CompositeDisposable Disposables = [];
    private readonly Settings _settings;

    protected UutSenderService(
        ILogger logger,
        ISfcService sfcService,
        Settings settings,
        SfcResponseBuilder sfcResponseBuilder)
    {
        _logger = logger;
        _settings = settings;
        _sfcResponseBuilder = sfcResponseBuilder;
        _sfcService = sfcService;
    }

    private void SetupReactiveObservers()
    {
        var isRunningDisposable = IsRunning
            .SkipWhile(x => x == false)
            .Subscribe(isRunning => _logger.Info($"UutSenderService {(isRunning ? "started" : "stopped")}"));

        this.Disposables.Add(isRunningDisposable);
    }

    public void Start()
    {
        try
        {
            if (IsRunning.Value) return;
            this.IsRunning.Value = true;
            this.State.Value = StateType.Idle;
            this.StartService();
            this.SetupReactiveObservers();
        }
        catch (Exception)
        {
            this.Stop();
            throw;
        }
    }

    protected abstract void StartService();

    public void Stop()
    {
        if (!IsRunning.Value) return;
        this.IsRunning.Value = false;
        this.State.Value = StateType.Stopped;
        this.StopService();
        this.Disposables.DisposeItems();
    }

    protected abstract void StopService();

    protected Task<SfcResponse> SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        if (!_settings.SendRepairFile && unitUnderTest.IsFail)
        {
            return Task.FromResult(_sfcResponseBuilder
                .SetOkSfcResponse()
                .Build());

            // TODO: Move from here
            //unitUnderTest.Message = _session.Settings.Machine is MachineType.Spi
            //   ? Resources.msg_spi_repair
            //   : "";
        }

        return Observable
            .FromAsync(async () => await _sfcService.SendAsync(unitUnderTest))
            .Do(x => _logger.Debug($"SendUnitUnderTest {unitUnderTest.FileName}, SfcResponse: {x.ResponseType}"))
            .Retry(this._settings.MaxSfcRetries)
            .Catch<SfcResponse, Exception>(_ => Observable.Return(Models.SfcResponse.Null))
            .ToTask();
    }
}