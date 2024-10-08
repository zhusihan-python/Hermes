using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public class UutSenderService
{
    public event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    public event EventHandler<UnitUnderTest>? SfcResponse;
    public event EventHandler<bool>? RunStatusChanged;

    private readonly ISfcService _sfcService;
    private readonly FileService _fileService;
    protected readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly ConcurrentQueue<string> _pendingFiles = new();
    protected readonly ILogger Logger;
    protected readonly ISettingsRepository SettingsRepository;
    protected readonly Session Session;
    protected CancellationTokenSource? CancellationTokenSource;
    protected bool IsRunning;
    private int _retries = 0;

    public UutSenderService(
        Session session,
        ILogger logger,
        ISfcService sfcService,
        FileService fileService,
        ISettingsRepository settingsRepository,
        FolderWatcherService folderWatcherService,
        UnitUnderTestBuilder unitUnderTestBuilder,
        UnitUnderTestRepository unitUnderTestRepository,
        SfcResponseBuilder sfcResponseBuilder)
    {
        this.Session = session;
        this.Logger = logger;
        this._sfcService = sfcService;
        this._fileService = fileService;
        this.SettingsRepository = settingsRepository;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._folderWatcherService.FileCreated += this.OnFileCreated;
    }

    public virtual string Path => SettingsRepository.Settings.InputPath; 

    public virtual void Start()
    {
        if (IsRunning) return;
        this._folderWatcherService.Filter = "*" + this.SettingsRepository.Settings.InputFileExtension.GetDescription();
        this._folderWatcherService.Start(SettingsRepository.Settings.InputPath);
        this.CancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => this.ProcessFilesAsync(this.CancellationTokenSource.Token));
    }

    private async Task ProcessFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.OnRunStatusChanged(true);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.Session.IsUutProcessorIdle && this._pendingFiles.TryDequeue(out var fullPath))
                {
                    this.Logger.Debug($"Processing file: {fullPath}");
                    var unitUnderTest = await this.SendFileAsync(fullPath);
                    if (!unitUnderTest.IsNull)
                    {
                        this.OnSfcResponse(unitUnderTest);
                    }
                }
                else
                {
                    await Task.Delay(this.SettingsRepository.Settings.WaitDelayMilliseconds, cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                this.Logger.Error(e.Message);
#if DEBUG
                if (!this.CancellationTokenSource?.IsCancellationRequested ?? false)
                {
                    this.Stop();
                }
#endif
            }
        }
        finally
        {
            this.OnRunStatusChanged(false);
        }
    }

    private async Task<UnitUnderTest> SendFileAsync(string fullPath)
    {
        var backupFullPath = await this._fileService.MoveToBackupAndAppendDateToNameAsync(fullPath);
        var unitUnderTest = await this.BuildUnitUnderTest(backupFullPath);
        if (unitUnderTest.IsNull)
        {
            return UnitUnderTest.Null;
        }

        await SendUnitUnderTest(unitUnderTest);
        return unitUnderTest;
    }

    protected async Task SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        if (!SettingsRepository.Settings.SendRepairFile && unitUnderTest.IsFail)
        {
            unitUnderTest.SfcResponse = _sfcResponseBuilder.SetOkContent().Build();
            unitUnderTest.Message = SettingsRepository.Settings.Station is StationType.SpiBottom or StationType.SpiTop
                ? Resources.msg_spi_repair
                : "";
        }
        else
        {
            unitUnderTest.SfcResponse = await this._sfcService.SendAsync(unitUnderTest);
        }

        if (unitUnderTest.SfcResponse.IsTimeout && this._retries < this.SettingsRepository.Settings.MaxSfcRetries - 1)
        {
            // TODO: Enable retry again
            this._retries += 1;
            //this.Logger.Error($"Timeout: {backupFullPath} | retry: {this._retries}");
            //await this._fileService.CopyFromBackupToInputAsync(backupFullPath);
        }
        else
        {
            this._retries = 0;
            await _unitUnderTestRepository.SaveChangesAsync();
        }
    }

    private async Task<UnitUnderTest> BuildUnitUnderTest(string fullPath)
    {
        var unitUnderTest = await this._unitUnderTestBuilder.BuildAsync(fullPath);
        if (unitUnderTest.IsNull)
        {
            this.Logger.Error($"Invalid file: {fullPath}");
            await this._fileService.MoveToBackupAndAppendDateToNameAsync(fullPath);
        }
        else
        {
            this.OnUnitUnderTestCreated(unitUnderTest);
            await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
        }

        return unitUnderTest;
    }

    public virtual void Stop()
    {
        if (!IsRunning) return;
        this.CancellationTokenSource?.Cancel();
        this._folderWatcherService.Stop();
    }

    private void OnFileCreated(object? sender, string fullPath)
    {
        this.EnqueueFullPath(fullPath);
    }

    private void EnqueueFullPath(string fullPath, string message = "File enqueued")
    {
        Logger.Debug($"{message}: {fullPath}");
        this._pendingFiles.Enqueue(fullPath);
    }


    protected virtual void OnUnitUnderTestCreated(UnitUnderTest unitUnderTest)
    {
        UnitUnderTestCreated?.Invoke(this, unitUnderTest);
    }

    protected virtual void OnSfcResponse(UnitUnderTest unitUnderTest)
    {
        SfcResponse?.Invoke(this, unitUnderTest);
    }

    protected virtual void OnRunStatusChanged(bool isRunning)
    {
        this.IsRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this.Logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}