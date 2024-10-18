using Hermes.Builders;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;
using System;
using Reactive.Bindings;
using ReactiveUI;

namespace Hermes.Services;

public abstract class UutSenderService(
    ILogger logger,
    ISfcService sfcService,
    ISettingsRepository settingsRepository,
    SfcResponseBuilder sfcResponseBuilder)
    : IUutSenderService
{
    public ReactivePropertySlim<UutProcessorState> State { get; protected set; } = new();
    public event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    public event EventHandler<UnitUnderTest>? SfcResponse;
    public event EventHandler<bool>? RunStatusChanged;

    protected readonly ISettingsRepository SettingsRepository = settingsRepository;
    protected bool IsRunning;

    public abstract string Path { get; }
    public bool IsWaitingForDummy { get; set; }

    public abstract void Start();

    public abstract void Stop();

    protected async Task SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        var attempt = 0;
        var shouldRetry = false;
        do
        {
            if (shouldRetry)
            {
                var delay = this.SettingsRepository.Settings.WaitDelayMilliseconds;
                logger.Info($" Retry SendUnitUnderTest waiting {delay} ms for {unitUnderTest.FileName}");
                await Task.Delay((int)delay);
            }

            if (!SettingsRepository.Settings.SendRepairFile && unitUnderTest.IsFail)
            {
                unitUnderTest.SfcResponse = sfcResponseBuilder.SetOkSfcResponse().Build();
                unitUnderTest.Message = SettingsRepository.Settings.Machine is MachineType.Spi
                    ? Resources.msg_spi_repair
                    : "";
            }
            else
            {
                unitUnderTest.SfcResponse = await sfcService.SendAsync(unitUnderTest);
            }

            shouldRetry = unitUnderTest.SfcResponse.IsTimeout || unitUnderTest.SfcResponse.IsEndOfFileError;
            attempt++;
        } while (shouldRetry && attempt <= this.SettingsRepository.Settings.MaxSfcRetries);
    }

    protected void OnUnitUnderTestCreated(UnitUnderTest unitUnderTest)
    {
        UnitUnderTestCreated?.Invoke(this, unitUnderTest);
    }

    protected void OnSfcResponse(UnitUnderTest unitUnderTest)
    {
        SfcResponse?.Invoke(this, unitUnderTest);
    }

    protected void OnRunStatusChanged(bool isRunning)
    {
        this.IsRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}