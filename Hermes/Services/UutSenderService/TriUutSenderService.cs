using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using R3;
using System.Threading.Tasks;

namespace Hermes.Services.UutSenderService;

public class TriUutSenderService : UutSenderService
{
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger _logger;
    private readonly Settings _settings;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public TriUutSenderService(
        FolderWatcherService folderWatcherService,
        ILogger logger,
        ISfcService sfcService,
        Settings settings,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder)
        : base(logger, sfcService, settings, sfcResponseBuilder)
    {
        this._settings = settings;
        this._logger = logger;
        this._folderWatcherService = folderWatcherService;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    public override string Path => _settings.InputPath;

    private void SetupReactiveObservers()
    {
        var textDocumentCreatedDisposable = this._folderWatcherService
            .TextDocumentCreated
            .Select(this.SendUnitUnderTest)
            .Subscribe();

        this.Disposables.Add(textDocumentCreatedDisposable);
    }

    protected override void StartService()
    {
        this.SetupReactiveObservers();
        this._folderWatcherService.Start(
            _settings.InputPath,
            "*" + this._settings.InputFileExtension.GetDescription());
    }

    private async Task SendUnitUnderTest(TextDocument textDocument)
    {
        this._logger.Debug($"Processing file: {textDocument.FileName}");
        var unitUnderTest = this._unitUnderTestBuilder.Build(textDocument);
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {textDocument.FileName}");
            return;
        }

        unitUnderTest.SfcResponse = await SendUnitUnderTest(unitUnderTest);
        this.UnitUnderTest.Value = unitUnderTest;
    }

    protected override void StopService()
    {
        this._folderWatcherService.Stop();
    }
}