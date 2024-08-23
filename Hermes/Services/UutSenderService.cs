using Hermes.Builders;
using Hermes.Common;
using Hermes.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Hermes.Common.Extensions;
using Hermes.Language;
using Hermes.Repositories;

namespace Hermes.Services;

public class UutSenderService
{
    public event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    public event EventHandler<UnitUnderTest>? SfcResponse;
    public event EventHandler<bool>? RunStatusChanged;

    private readonly Session _session;
    private readonly ILogger _logger;
    private readonly ISfcService _sfcService;
    private readonly FileService _fileService;
    private readonly ISettingsRepository _settingsRepository;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly ConcurrentQueue<string> _pendingFiles = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
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
        this._session = session;
        this._logger = logger;
        this._sfcService = sfcService;
        this._fileService = fileService;
        this._settingsRepository = settingsRepository;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._folderWatcherService.FileCreated += this.OnFileCreated;
    }

    public void Start()
    {
        if (_isRunning) return;
        this._folderWatcherService.Filter = "*" + this._settingsRepository.Settings.InputFileExtension.GetDescription();
        this._folderWatcherService.Start(_settingsRepository.Settings.InputPath);
        this._cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => this.ProcessFilesAsync(this._cancellationTokenSource.Token));
    }

    private async Task ProcessFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.OnRunStatusChanged(true);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this._session.IsUutProcessorIdle && this._pendingFiles.TryDequeue(out var fullPath))
                {
                    this._logger.Debug($"Processing file: {fullPath}");
                    var unitUnderTest = await this.SendFileAsync(fullPath);
                    if (!unitUnderTest.IsNull)
                    {
                        this.OnSfcResponse(unitUnderTest);
                    }
                }
                else
                {
                    await Task.Delay(this._settingsRepository.Settings.WaitDelayMilliseconds, cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                this._logger.Error(e.Message);
#if DEBUG
                if (!this._cancellationTokenSource?.IsCancellationRequested ?? false)
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
        var backupFullPath = await this._fileService.MoveToBackupAsync(fullPath);
        var unitUnderTest = await this.BuildUnitUnderTest(backupFullPath);
        if (unitUnderTest.IsNull)
        {
            return UnitUnderTest.Null;
        }

        if (!_settingsRepository.Settings.SendRepairFile && unitUnderTest.IsFail)
        {
            unitUnderTest.SfcResponse = _sfcResponseBuilder.SetOkContent().Build();
        }
        else
        {
            unitUnderTest.SfcResponse = await this._sfcService.SendAsync(unitUnderTest);
            unitUnderTest.Message = Resources.msg_spi_repair;
        }

        if (unitUnderTest.SfcResponse.IsTimeout && this._retries < this._settingsRepository.Settings.MaxSfcRetries - 1)
        {
            this._retries += 1;
            this._logger.Error($"Timeout: {backupFullPath} | retry: {this._retries}");
            await this._fileService.CopyFromBackupToInputAsync(backupFullPath);
        }
        else
        {
            this._retries = 0;
            await _unitUnderTestRepository.SaveChangesAsync();
        }

        return unitUnderTest;
    }

    private async Task<UnitUnderTest> BuildUnitUnderTest(string fullPath)
    {
        var unitUnderTest = await this._unitUnderTestBuilder.BuildAsync(fullPath);
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {fullPath}");
            await this._fileService.MoveToBackupAsync(fullPath);
        }
        else
        {
            this.OnUnitUnderTestCreated(unitUnderTest);
            await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
        }

        return unitUnderTest;
    }

    public void Stop()
    {
        if (!_isRunning) return;
        this._cancellationTokenSource?.Cancel();
        this._folderWatcherService.Stop();
    }

    private void OnFileCreated(object? sender, string fullPath)
    {
        this.EnqueueFullPath(fullPath);
    }

    private void EnqueueFullPath(string fullPath, string message = "File enqueued")
    {
        _logger.Debug($"{message}: {fullPath}");
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
        this._isRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this._logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}