using Hermes.Builders;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using Hermes.Types;
using R3;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class SfcSimulatorService(
    FileService fileService,
    FolderWatcherService folderWatcherService,
    ILogger logger,
    ParserPrototype parserPrototype,
    Settings settings,
    SfcResponseBuilder sfcResponseBuilder)
{
    public SfcResponseType Mode { get; set; } = SfcResponseType.Ok;

    private DisposableBag _disposables;
    public readonly ReactiveProperty<bool> IsRunning = new();

    public void Start()
    {
        try
        {
            if (IsRunning.Value) return;
            IsRunning.Value = true;
            SetupReactiveObservers();
            folderWatcherService.Start(
                settings.SfcPath,
                settings.InputFileFilter);
        }
        catch (Exception)
        {
            Stop();
            throw;
        }
    }

    private void SetupReactiveObservers()
    {
        folderWatcherService
            .TextDocumentCreated
            .DistinctBy(x => x.FullPath)
            .Select(SendSfcResponse)
            .Subscribe()
            .AddTo(ref _disposables);

        IsRunning
            .Subscribe(isRunning => { logger.Info($"Sfc simulator {(isRunning ? "started" : "stopped")}"); })
            .AddTo(ref _disposables);
    }

    public void Stop()
    {
        if (!IsRunning.Value) return;
        IsRunning.Value = false;
        folderWatcherService.Stop();
        _disposables.Clear();
    }

    private async Task SendSfcResponse(TextDocument textDocument)
    {
        logger.Debug($"SfcSimulator Process: {textDocument.FileName} | Mode: {Mode}");
        await fileService.DeleteFileIfExists(textDocument.FullPath);
        if (Mode == SfcResponseType.Timeout)
        {
            return;
        }

        await fileService.WriteSfcResponseAsync(
            textDocument.FileNameWithoutExtension,
            GetSfcResponseContent(textDocument));
        logger.Debug($"SfcSimulator Responded");
    }

    private string GetSfcResponseContent(TextDocument textDocument)
    {
        switch (Mode)
        {
            case SfcResponseType.WrongStation:
                sfcResponseBuilder.SetWrongStation();
                break;
            case SfcResponseType.Unknown:
                sfcResponseBuilder.SetUnknownContent();
                break;
            default:
                sfcResponseBuilder.SetOkSfcResponse();
                break;
        }

        sfcResponseBuilder.SerialNumber(GetSerialNumber(textDocument));

        return sfcResponseBuilder.GetContent();
    }

    private string GetSerialNumber(TextDocument textDocument)
    {
        var parser = parserPrototype.GetUnitUnderTestParser(settings.LogfileType);
        return parser == null
            ? ""
            : parser.ParseSerialNumber(textDocument.Content).ToUpper();
    }
}