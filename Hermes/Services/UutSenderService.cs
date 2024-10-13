using Hermes.Builders;
using Hermes.Common.Aspects;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public abstract class UutSenderService(
    ILogger logger,
    ISfcService sfcService,
    ISettingsRepository settingsRepository,
    SfcResponseBuilder sfcResponseBuilder)
    : IUutSenderService
{
    public event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    public event EventHandler<UnitUnderTest>? SfcResponse;
    public event EventHandler<bool>? RunStatusChanged;

    protected readonly ILogger Logger = logger;
    protected readonly ISettingsRepository SettingsRepository = settingsRepository;
    protected bool IsRunning;

    public abstract string Path { get; }

    public abstract void Start();

    public abstract void Stop();

    [Retry(Attempts = 2, Delay = 500)]
    protected async Task SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        if (!SettingsRepository.Settings.SendRepairFile && unitUnderTest.IsFail)
        {
            unitUnderTest.SfcResponse = sfcResponseBuilder.SetOkContent().Build();
            unitUnderTest.Message = SettingsRepository.Settings.Machine is MachineType.Spi
                ? Resources.msg_spi_repair
                : "";
        }
        else
        {
            unitUnderTest.SfcResponse = await sfcService.SendAsync(unitUnderTest);
        }

        if (unitUnderTest.SfcResponse.IsTimeout || unitUnderTest.SfcResponse.IsEndOfFileError) 
        {
            unitUnderTest.SfcResponse = Models.SfcResponse.Null;
            throw new TimeoutException();
        }
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
        this.Logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}