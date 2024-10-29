using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services.UutSenderService;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using R3;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _stateText = Resources.enum_stopped;
    [ObservableProperty] private bool _isWaitingForDummy;
    public ReactiveProperty<UnitUnderTest> CurrentUnitUnderTest { get; } = new(Models.UnitUnderTest.Null);
    public ScannerViewModel ScannerViewModel { get; }
    public DummyViewModel DummyViewModel { get; }

    private readonly FileService _fileService;
    private readonly ILogger _logger;
    private readonly Session _session;
    private readonly StopService _stopService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private readonly UutSenderService _uutSenderService;

    public UutProcessorViewModel(
        ILogger logger,
        Session session,
        StopService stopService,
        FileService fileService,
        UutSenderServiceFactory uutSenderServiceFactory,
        UnitUnderTestRepository unitUnderTestRepository,
        ScannerViewModel scannerViewModel,
        DummyViewModel dummyViewModel)
        : base(
            Resources.txt_uut_processor,
            MaterialIconKind.FolderEye,
            PermissionType.OpenUutProcessor,
            1)
    {
        this.DummyViewModel = dummyViewModel;
        this.ScannerViewModel = scannerViewModel;
        this._fileService = fileService;
        this._logger = logger;
        this._session = session;
        this._stopService = stopService;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._uutSenderService = uutSenderServiceFactory.Build();
        this.StationFilter = EnumExtensions.GetValues<StationType>()
            .Where(x => x != StationType.Labeling && x != StationType.None)
            .ToList();
        this.CurrentUnitUnderTest = this._uutSenderService
            .UnitUnderTest
            .ToBindableReactiveProperty<UnitUnderTest>();
        this.SetupReactiveExtensionsOnActivation = false;
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {
        this._uutSenderService
            .State
            .Do(x => this._session.UutProcessorState.Value = x)
            .Do(x => this.StateText = x.ToTranslatedString())
            .SkipWhile(x => x == StateType.Stopped)
            .Do(x =>
            {
                if (x == StateType.Stopped)
                {
                    this.Stop();
                }
            })
            .Subscribe()
            .AddTo(ref Disposables);

        this._uutSenderService
            .UnitUnderTest
            .Where(unitUnderTest => !unitUnderTest.IsNull)
            .SelectAwait(async (unitUnderTest, _) =>
                (stop: await this._stopService.Calculate(unitUnderTest), unitUnderTest))
            .Do(x => this.ShowResult(x.stop, x.unitUnderTest))
            .Subscribe()
            .AddTo(ref Disposables);

        this._uutSenderService
            .UnitUnderTest
            .Where(unitUnderTest => !unitUnderTest.IsNull)
            .Distinct()
            .SubscribeAwait(async (unitUnderTest, _) =>
            {
                await this.MoveFilesToBackup(unitUnderTest);
                await this.Persist(unitUnderTest);
            })
            .AddTo(ref Disposables);
    }

    protected override void OnActivated()
    {
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        Messenger.Register<WaitForDummyMessage>(this, this.OnWaitForDummyMessage);
        Messenger.Register<ReSendUnitUnderTestMessage>(this, this.OnReSendUnitUnderTestMessage);
        base.OnActivated();
    }

    private void OnReSendUnitUnderTestMessage(object recipient, ReSendUnitUnderTestMessage message)
    {
        if (this._uutSenderService.CanReSend(message.Value))
        {
            this._uutSenderService.ReSend(message.Value);
        }
        else
        {
            this.ShowErrorToast(Resources.msg_can_not_resed_uut);
        }
    }

    private void OnWaitForDummyMessage(object recipient, WaitForDummyMessage message)
    {
        this._uutSenderService.IsWaitingForDummy = message.Value;
    }

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        base.OnDeactivated();
    }

    [RelayCommand]
    private void Start()
    {
        try
        {
            if (this.IsRunning) return;
            this.IsRunning = true;
            this.Path = this._uutSenderService.Path;
            this._uutSenderService.Start();
            this._stopService.Start();
            this.SetupReactiveExtensions();
            this.ShowInfoToast(Resources.msg_uut_processor_started);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    [RelayCommand]
    private void Stop()
    {
        try
        {
            if (!this.IsRunning) return;
            this.IsRunning = false;
            this.Path = this._uutSenderService.Path;
            this._uutSenderService.Stop();
            this._stopService.Stop();
            this.Disposables.Clear();
            this.ShowInfoToast(Resources.msg_uut_processor_stopped);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    private void ShowResult(Stop stop, UnitUnderTest unitUnderTest)
    {
        if (stop.IsNull)
        {
            Messenger.Send(new ShowSuccessMessage(unitUnderTest));
        }
        else
        {
            _session.Stop = stop;
            Messenger.Send(new ShowStopMessage(stop));
        }
    }

    private async Task MoveFilesToBackup(UnitUnderTest unitUnderTest)
    {
        unitUnderTest.FullPath = await this._fileService
            .MoveToBackupAsync(unitUnderTest.FullPath);
        unitUnderTest.SfcResponseFullPath = await this._fileService
            .MoveToBackupAsync(unitUnderTest.SfcResponseFullPath);
    }

    private async Task Persist(UnitUnderTest unitUnderTest)
    {
        await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
    }

    private void OnStartReceive(object recipient, StartUutProcessorMessage message)
    {
        this.Start();
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }
}