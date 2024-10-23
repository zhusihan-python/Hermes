using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Types;
using R3;
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
    protected DisposableBag Disposables;
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
        IsRunning
            .SkipWhile(x => x == false)
            .Subscribe(isRunning => _logger.Info($"UutSenderService {(isRunning ? "started" : "stopped")}"))
            .AddTo(ref Disposables);
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
        this.Disposables.Clear();
    }

    protected abstract void StopService();

    protected async Task<SfcResponse> SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        if (!_settings.SendRepairFile && unitUnderTest.IsFail)
        {
            // TODO: Move from here
            unitUnderTest.Message = _settings.Machine is MachineType.Spi
                ? Resources.msg_spi_repair
                : "";

            return _sfcResponseBuilder
                .SetOkSfcResponse()
                .Build();
        }

        // TODO: Is there a better way to do this?
        var i = 0;
        var sfcResponse = SfcResponse.Null;
        do
        {
            try
            {
                sfcResponse = await _sfcService.SendAsync(unitUnderTest);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                i++;
            }
        } while (i < _settings.MaxSfcRetries && sfcResponse.IsTimeout);

        _logger.Debug($"SendUnitUnderTest {unitUnderTest.FileName}, SfcResponse: {sfcResponse.ResponseType}");
        return sfcResponse;
    }
}