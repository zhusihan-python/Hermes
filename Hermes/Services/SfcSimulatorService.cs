using Hermes.Builders;
using Hermes.Common.Parsers;
using Hermes.Common;
using Hermes.Models;
using Hermes.Types;
using Reactive.Bindings;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class SfcSimulatorService(
    FileService fileService,
    FolderWatcherService folderWatcherService,
    ILogger logger,
    ParserPrototype parserPrototype,
    Session session,
    SfcResponseBuilder sfcResponseBuilder)
{
    public SfcResponseType Mode { get; set; } = SfcResponseType.Ok;

    private readonly CompositeDisposable _disposables = [];
    public readonly ReactiveProperty<bool> IsRunning = new();

    public void Start()
    {
        try
        {
            if (IsRunning.Value) return;
            IsRunning.Value = true;
            SetupReactiveObservers();
            folderWatcherService.Start(
                session.Settings.SfcPath,
                session.Settings.InputFileFilter);
        }
        catch (Exception)
        {
            Stop();
            throw;
        }
    }

    private void SetupReactiveObservers()
    {
        var textDocumentCreatedDisposable = folderWatcherService
            .TextDocumentCreated
            .Distinct(x => x.FullPath)
            .Select(SendSfcResponse)
            .Subscribe();

        var isRunningDisposable = IsRunning
            .Subscribe(isRunning => { logger.Info($"Sfc simulator {(isRunning ? "started" : "stopped")}"); });

        _disposables.Add(textDocumentCreatedDisposable);
        _disposables.Add(isRunningDisposable);
    }

    public void Stop()
    {
        if (!IsRunning.Value) return;
        IsRunning.Value = false;
        folderWatcherService.Stop();
        _disposables.Dispose();
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
        var parser = parserPrototype.GetUnitUnderTestParser(session.Settings.LogfileType);
        return parser == null
            ? ""
            : parser.ParseSerialNumber(textDocument.Content).ToUpper();
    }
}