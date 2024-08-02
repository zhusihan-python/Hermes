using Hermes.Builders;
using Hermes.Common;
using Hermes.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Hermes.Services;

public class UutSenderService
{
    public event EventHandler<UnitUnderTest>? UnitUnderTestCreated;
    public event EventHandler<SfcResponse>? SfcResponseCreated;
    public event EventHandler<bool>? RunStatusChanged;

    private readonly Session _session;
    private readonly ILogger _logger;
    private readonly Settings _settings;
    private readonly SfcService _sfcService;
    private readonly FileService _fileService;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ConcurrentQueue<string> _pendingFiles = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
    private int _retries = 0;

    public UutSenderService(
        Session session,
        ILogger logger,
        Settings settings,
        SfcService sfcService,
        FileService fileService,
        FolderWatcherService folderWatcherService,
        UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._session = session;
        this._logger = logger;
        this._settings = settings;
        this._sfcService = sfcService;
        this._fileService = fileService;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._folderWatcherService.FileCreated += this.OnFileCreated;
    }

    public void Start()
    {
        if (_isRunning) return;
        this._folderWatcherService.Start();
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
                    var sfcResponse = await this.SendFileToSfcAsync(fullPath);
                    if (!sfcResponse.IsNull)
                    {
                        this.OnSfcResponseCreated(sfcResponse);
                    }
                }
                else
                {
                    await Task.Delay(this._settings.WaitDelayMilliseconds, cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                this._logger.Error(e.Message);
                if (!this._cancellationTokenSource?.IsCancellationRequested ?? false)
                {
                    this.Stop();
                }
            }
        }
        finally
        {
            this.OnRunStatusChanged(false);
        }
    }

    public async Task<SfcResponse> SendFileToSfcAsync(string fullPath)
    {
        var unitUnderTest = await this.BuildUnitUnderTest(fullPath);
        if (unitUnderTest.IsNull)
        {
            return SfcResponse.Null;
        }

        var sfcResponse = await this._sfcService.SendAsync(unitUnderTest);
        if (sfcResponse.IsTimeout && this._retries < this._settings.MaxSfcRetries - 1)
        {
            this.EnqueueFullPath(fullPath);
            this._retries += 1;
            this._logger.Error($"Timeout: {fullPath} | retry: {this._retries}");
        }
        else
        {
            this._retries = 0;
        }

        return sfcResponse;
    }

    private async Task<UnitUnderTest> BuildUnitUnderTest(string fullPath)
    {
        var unitUnderTest = await this._unitUnderTestBuilder.BuildAsync(fullPath);
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {fullPath}");
            await this._fileService.MoveToBackupAsync(fullPath);
            return UnitUnderTest.Null;
        }
        
        await this._fileService.CopyToBackupAsync(fullPath);
        this.OnUnitUnderTestCreated(unitUnderTest);
        return unitUnderTest;
    }

    private void EnqueueFullPath(string fullPath)
    {
        this.OnFileCreated(this, fullPath);
        this._pendingFiles.Enqueue(fullPath);
    }

    public void Stop()
    {
        if (!_isRunning) return;
        this._cancellationTokenSource?.Cancel();
        this._folderWatcherService.Stop();
    }

    private void OnFileCreated(object? sender, string fullPath)
    {
        _logger.Debug("File created: " + fullPath);
        this._pendingFiles.Enqueue(fullPath);
    }

    protected virtual void OnUnitUnderTestCreated(UnitUnderTest unitUnderTest)
    {
        UnitUnderTestCreated?.Invoke(this, unitUnderTest);
    }

    protected virtual void OnSfcResponseCreated(SfcResponse sfcResponse)
    {
        SfcResponseCreated?.Invoke(this, sfcResponse);
    }

    protected virtual void OnRunStatusChanged(bool isRunning)
    {
        this._isRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this._logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}