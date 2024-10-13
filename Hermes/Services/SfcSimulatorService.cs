using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class SfcSimulatorService
{
    public SfcResponseType Mode { get; set; } = SfcResponseType.Ok;
    public event EventHandler<bool>? RunStatusChanged;

    private bool _isRunning;
    private readonly FileService _fileService;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger _logger;
    private readonly ISettingsRepository _settingsRepository;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly ParserPrototype _parserPrototype;
    private string _lastProcessedFile = "";


    public SfcSimulatorService(
        ILogger logger,
        FileService fileService,
        ISettingsRepository settingsRepository,
        ParserPrototype parserPrototype,
        SfcResponseBuilder sfcResponseBuilder,
        FolderWatcherService folderWatcherService
    )
    {
        this._logger = logger;
        this._fileService = fileService;
        this._settingsRepository = settingsRepository;
        this._parserPrototype = parserPrototype;
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._folderWatcherService = folderWatcherService;
    }

    public void Start()
    {
        if (_isRunning) return;
        this._folderWatcherService.Filter = "*" + _settingsRepository.Settings.InputFileExtension.GetDescription();
        this._folderWatcherService.FileCreated += this.OnFileCreated;
        this._folderWatcherService.Start(_settingsRepository.Settings.SfcPath);
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
        if (_lastProcessedFile == fullPath) return;
        _lastProcessedFile = fullPath;
        _logger.Info($"SfcSimulator Process: {fullPath} | Mode: {this.Mode}");
        var content = await this.GetContent(fullPath);
        await this._fileService.DeleteFileIfExists(fullPath);
        if (this.Mode == SfcResponseType.Timeout)
        {
            return;
        }

        await this._fileService.WriteAllTextAsync(
            SfcRequest.GetResponseFullpath(fullPath, this._settingsRepository.Settings.SfcResponseExtension),
            content
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
            (await this.GetSerialNumber(fullPath))
            .ToUpper());

        return this._sfcResponseBuilder.GetContent();
    }

    private async Task<string> GetSerialNumber(string fullPath)
    {
        var content = await this._fileService.TryReadAllTextAsync(fullPath);
        var parser = _parserPrototype.GetUnitUnderTestParser(_settingsRepository.Settings.LogfileType);
        return parser == null ? "" : parser.ParseSerialNumber(content);
    }

    protected virtual void OnRunStatusChanged(bool isRunning)
    {
        this._isRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this._logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}