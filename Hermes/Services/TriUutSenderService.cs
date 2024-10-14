using Hermes.Builders;
using Hermes.Common.Aspects;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public class TriUutSenderService : UutSenderService
{
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ConcurrentQueue<string> _pendingFiles = new();
    private readonly FileService _fileService;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger _logger;
    private readonly ISettingsRepository _settingsRepository;
    private readonly Session _session;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

    public TriUutSenderService(
        FileService fileService,
        FolderWatcherService folderWatcherService,
        ILogger logger,
        ISettingsRepository settingsRepository,
        ISfcService sfcService,
        Session session,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder,
        UnitUnderTestRepository unitUnderTestRepository)
        : base(logger, sfcService, settingsRepository, sfcResponseBuilder)
    {
        this._session = session;
        this._logger = logger;
        this._fileService = fileService;
        this._settingsRepository = settingsRepository;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._folderWatcherService.FileCreated += this.OnFileCreated;
    }

    public override string Path => _settingsRepository.Settings.InputPath;

    public override void Start()
    {
        try
        {
            if (IsRunning) return;
            this.IsRunning = true;
            this._folderWatcherService.Filter =
                "*" + this._settingsRepository.Settings.InputFileExtension.GetDescription();
            this._folderWatcherService.Start(_settingsRepository.Settings.InputPath);
            this._cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => this.ProcessFilesAsync(this._cancellationTokenSource.Token));
        }
        catch (Exception)
        {
            this.Stop();
            throw;
        }
    }

    private async Task ProcessFilesAsync(CancellationToken cancellationToken)
    {
        this.OnRunStatusChanged(true);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
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
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    this._logger.Error(e.Message);
                }
            }
        }

        this.OnRunStatusChanged(false);
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
        await _unitUnderTestRepository.SaveChangesAsync();
        return unitUnderTest;
    }

    private async Task<UnitUnderTest> BuildUnitUnderTest(string fullPath)
    {
        var unitUnderTest = await this._unitUnderTestBuilder.BuildAsync(fullPath);
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {fullPath}");
            await this._fileService.MoveToBackupAndAppendDateToNameAsync(fullPath);
        }
        else
        {
            this.OnUnitUnderTestCreated(unitUnderTest);
            await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
        }

        return unitUnderTest;
    }

    public override void Stop()
    {
        if (!IsRunning) return;
        this.IsRunning = false;
        this._cancellationTokenSource?.Cancel();
        this._folderWatcherService.Stop();
    }

    private void OnFileCreated(object? sender, string fullPath)
    {
        _logger.Debug($"File enqueued: {fullPath}");
        this._pendingFiles.Enqueue(fullPath);
    }
}