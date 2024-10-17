using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Reactive.Linq;
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
        this.SetupReactiveExtensions();
    }

    private void SetupReactiveExtensions()
    {
        this._folderWatcherService.TextDocumentCreated
            .Distinct(x => x.FullPath)
            .Select(async (x) => await this.OnTextDocumentCreated(x))
            .Subscribe();
    }

    public void Start()
    {
        if (_isRunning) return;
        this._folderWatcherService.Start(
            _settingsRepository.Settings.SfcPath,
            "*" + _settingsRepository.Settings.InputFileExtension.GetDescription());
        this.OnRunStatusChanged(true);
    }

    public void Stop()
    {
        if (!_isRunning) return;
        this._folderWatcherService.Stop();
        this.OnRunStatusChanged(false);
    }

    private async Task OnTextDocumentCreated(TextDocument textDocument)
    {
        _logger.Info($"SfcSimulator Process: {textDocument.FileName} | Mode: {this.Mode}");
        await this._fileService.DeleteFileIfExists(textDocument.FullPath);
        if (this.Mode == SfcResponseType.Timeout)
        {
            return;
        }

        await this._fileService.WriteSfcResponseAsync(
            textDocument.FileNameWithoutExtension,
            this.GetContent(textDocument));
    }

    private string GetContent(TextDocument textDocument)
    {
        switch (this.Mode)
        {
            case SfcResponseType.WrongStation:
                this._sfcResponseBuilder.SetWrongStation();
                break;
            case SfcResponseType.Unknown:
                this._sfcResponseBuilder.SetUnknownContent();
                break;
            default:
                this._sfcResponseBuilder.SetOkSfcResponse();
                break;
        }

        this._sfcResponseBuilder.SerialNumber(
            this.GetSerialNumber(textDocument));

        return this._sfcResponseBuilder.GetContent();
    }

    private string GetSerialNumber(TextDocument textDocument)
    {
        var parser = _parserPrototype.GetUnitUnderTestParser(_settingsRepository.Settings.LogfileType);
        return parser == null
            ? ""
            : parser.ParseSerialNumber(textDocument.Content).ToUpper();
    }

    protected virtual void OnRunStatusChanged(bool isRunning)
    {
        this._isRunning = isRunning;
        RunStatusChanged?.Invoke(this, isRunning);
        this._logger.Info($"SfcSenderService {(isRunning ? "started" : "stopped")}");
    }
}