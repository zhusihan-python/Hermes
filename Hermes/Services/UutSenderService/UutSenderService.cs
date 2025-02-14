using Hermes.Builders;
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
    public ReactiveProperty<UnitUnderTest> UnitUnderTest { get; } = new(Models.UnitUnderTest.Null);

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

    protected abstract void SetupReactiveExtensions();

    public void Start()
    {
        try
        {
            if (this.State.Value.IsRunning()) return;
            this.State.Value = StateType.Idle;
            this.StartService();
            this.SetupReactiveExtensions();
            _logger.Info($"UutSenderService started");
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
        if (!this.State.Value.IsRunning()) return;
        this.State.Value = StateType.Stopped;
        this.StopService();
        this.Disposables.Clear();
        _logger.Info($"UutSenderService stopped");
    }

    protected abstract void StopService();

    protected async Task<SfcResponse> SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        if (!_settings.SendRepairFile && unitUnderTest.IsFail)
        {
            unitUnderTest.Message = _settings.Machine is MachineType.Spi
                ? Resources.msg_spi_repair
                : "";

            return _sfcResponseBuilder
                .SetOkSfcResponse()
                .Build();
        }

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
            }

            i++;
        } while (i <= _settings.MaxSfcRetries && sfcResponse.IsTimeout);

        _logger.Debug($"SendUnitUnderTest {unitUnderTest.FileName}, SfcResponse: {sfcResponse.Type}");
        return sfcResponse;
    }

    public abstract Task ReSend(UnitUnderTest unitUnderTest);
    public abstract bool CanReSend(UnitUnderTest unitUnderTest);
}