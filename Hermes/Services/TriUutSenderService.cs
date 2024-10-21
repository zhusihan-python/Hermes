using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class TriUutSenderService : UutSenderService
{
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger _logger;
    private readonly Session _session;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public TriUutSenderService(
        FolderWatcherService folderWatcherService,
        ILogger logger,
        ISfcService sfcService,
        Session session,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder)
        : base(logger, sfcService, session, sfcResponseBuilder)
    {
        this._session = session;
        this._logger = logger;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    public override string Path => _session.Settings.InputPath;

    private void SetupReactiveObservers()
    {
        var textDocumentCreatedDisposable = this._folderWatcherService
            .TextDocumentCreated
            .Select(this.SendTextDocument)
            .Subscribe();

        this.Disposables.Add(textDocumentCreatedDisposable);
    }

    protected override void StartService()
    {
        this.SetupReactiveObservers();
        this._folderWatcherService.Start(
            _session.Settings.InputPath,
            "*" + this._session.Settings.InputFileExtension.GetDescription());
    }

    private async Task SendTextDocument(TextDocument textDocument)
    {
        this._logger.Debug($"Processing file: {textDocument.FileName}");
        var unitUnderTest = this._unitUnderTestBuilder.Build(textDocument);
        this.UnitUnderTestCreated.Value = unitUnderTest;
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {textDocument.FileName}");
            return;
        }

        var sfcResponse = await SendUnitUnderTest(unitUnderTest);
        this.SfcResponseCreated.Value = sfcResponse;
    }

    protected override void StopService()
    {
        this._folderWatcherService.Stop();
    }
}