using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Reactive.Linq;

namespace Hermes.Services;

public class TriUutSenderService : UutSenderService
{
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
        this.SetupReactiveExtensions();
    }

    public override string Path => _settingsRepository.Settings.InputPath;

    private void SetupReactiveExtensions()
    {
        this._folderWatcherService.TextDocumentCreated
            .Distinct(x => x.FullPath)
            .Select(x =>
            {
                Console.WriteLine($"File created: {x.FullPath}");
                return x;
            })
            .Select(async (x) => await this.OnTextDocumentCreated(x))
            .Subscribe();
    }

    public override void Start()
    {
        try
        {
            if (IsRunning) return;
            this.IsRunning = true;
            this._folderWatcherService.Start(
                _settingsRepository.Settings.InputPath,
                "*" + this._settingsRepository.Settings.InputFileExtension.GetDescription());
            this.OnRunStatusChanged(true);
        }
        catch (Exception)
        {
            this.Stop();
            throw;
        }
    }

    private async Task OnTextDocumentCreated(TextDocument textDocument)
    {
        if (this._session.IsUutProcessorIdle)
        {
            this._logger.Debug($"Processing file: {textDocument.FileName}");
            var unitUnderTest = await this.SendFileAsync(textDocument);
            if (!unitUnderTest.IsNull)
            {
                this.OnSfcResponse(unitUnderTest);
            }
        }
    }

    private async Task<UnitUnderTest> SendFileAsync(TextDocument textDocument)
    {
        await this._fileService.MoveToBackupAndAppendDateToNameAsync(textDocument.FullPath);
        var unitUnderTest = await this.BuildUnitUnderTest(textDocument);
        if (unitUnderTest.IsNull)
        {
            return UnitUnderTest.Null;
        }

        await SendUnitUnderTest(unitUnderTest);
        await _unitUnderTestRepository.SaveChangesAsync();
        return unitUnderTest;
    }

    private async Task<UnitUnderTest> BuildUnitUnderTest(TextDocument textDocument)
    {
        var unitUnderTest = this._unitUnderTestBuilder.Build(textDocument);
        if (unitUnderTest.IsNull)
        {
            this._logger.Error($"Invalid file: {textDocument.FileName}");
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
        this._folderWatcherService.Stop();
        this.OnRunStatusChanged(false);
    }
}