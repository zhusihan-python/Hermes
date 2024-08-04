using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Builders;
using Hermes.Common;
using Hermes.Common.Extensions;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Services;

public class SfcSimulatorService
{
    public SfcResponseType Mode { get; set; } = SfcResponseType.Ok;
    public event EventHandler<bool>? RunStatusChanged;

    private bool _isRunning;
    private readonly FileService _fileService;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger _logger;
    private readonly Settings _settings;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;


    public SfcSimulatorService(
        ILogger logger,
        Settings settings,
        FileService fileService,
        UnitUnderTestBuilder unitUnderTestBuilder,
        SfcResponseBuilder sfcResponseBuilder,
        FolderWatcherService folderWatcherService
    )
    {
        this._logger = logger;
        this._settings = settings;
        this._fileService = fileService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._folderWatcherService = folderWatcherService;
    }

    public void Start()
    {
        if (_isRunning) return;
        this._folderWatcherService.Start(_settings.SfcPath);
        this._folderWatcherService.Filter = "*" + _settings.InputFileExtension.GetDescription();
        this._folderWatcherService.FileCreated += this.OnFileCreated;
        this.OnRunStatusChanged(true);
    }

    public void Stop()
    {
        if (!_isRunning) return;
        this._folderWatcherService.Stop();
        this._folderWatcherService.FileCreated -= this.OnFileCreated;
        this.OnRunStatusChanged(false);
    }

    private async void OnFileCreated(object? sender, string fullPath)
    {
        await this.Process(fullPath);
    }


    private async Task Process(string fullPath)
    {
        _logger.Info($"SfcSimulator Process: {fullPath} | Mode: {this.Mode}");
        await this._fileService.DeleteFileIfExists(fullPath);
        if (this.Mode == SfcResponseType.Timeout)
        {
            return;
        }

        await this._fileService.WriteAllTextAsync(
            SfcRequest.GetResponseFullpath(fullPath, this._settings.SfcResponseExtension),
            await this.GetContent(fullPath)
        );
    }

    private async Task<string> GetContent(string fullPath)
    {
        if (this.Mode == SfcResponseType.WrongStation)
            this._sfcResponseBuilder.SetWrongStation();
        else if (this.Mode == SfcResponseType.Unknown)
            this._sfcResponseBuilder.SetUnknownContent();
        else
            this._sfcResponseBuilder.SetOkContent();

        this._sfcResponseBuilder.SerialNumber(
            (await this._unitUnderTestBuilder.BuildAsync(fullPath)).SerialNumber);

        return this._sfcResponseBuilder.GetContent();
    }

    protected virtual void OnRunStatusChanged(bool isRunning)
    {
        this._isRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this._logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}