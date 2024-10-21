using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using Reactive.Bindings.Disposables;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _stateText = Resources.enum_stopped;
    [ObservableProperty] private bool _isWaitingForDummy;
    [ObservableProperty] private UnitUnderTest _currentUnitUnderTest = UnitUnderTest.Null;
    public ScannerViewModel ScannerViewModel { get; }
    public DummyViewModel DummyViewModel { get; }

    private UutSenderService? _uutSenderService;
    private readonly CompositeDisposable _disposables = [];
    private readonly FileService _fileService;
    private readonly ILogger _logger;
    private readonly Session _session;
    private readonly StopService _stopService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private readonly UutSenderServiceFactory _uutSenderServiceFactory;

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
        this._uutSenderServiceFactory = uutSenderServiceFactory;
        this.StationFilter = EnumExtensions.GetValues<StationType>()
            .Where(x => x != StationType.Labeling && x != StationType.None)
            .ToList();
        this.IsActive = true;
    }

    private void SetupReactiveObservers()
    {
        this._uutSenderService = this._uutSenderServiceFactory.Build();
        var uutSenderServiceIsRunningChangeDisposable = this._uutSenderService
            .IsRunning
            .Do(isRunning =>
            {
                if (!isRunning) this.Stop();
            })
            .Subscribe();

        var uutSenderServiceStateChangeDisposable = this._uutSenderService
            .State
            .Do(x => this._session.UutProcessorCurrentState.Value = x)
            .Do(x => this.StateText = x.ToTranslatedString())
            .Subscribe();

        var uutCreatedDisposable = this._uutSenderService
            .UnitUnderTestCreated
            .SubscribeOn(SynchronizationContext.Current!)
            .Where(unitUnderTest => !unitUnderTest.IsNull)
            .Do(unitUnderTest => this.CurrentUnitUnderTest = unitUnderTest)
            .Select(async unitUnderTest => await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest))
            .Subscribe();

        var uutResponseCreatedDisposable = this._uutSenderService
            .SfcResponseCreated
            .SubscribeOn(SynchronizationContext.Current!)
            .Where(sfcResponse => !sfcResponse.IsNull)
            .Do(sfcResponse => this.CurrentUnitUnderTest.SfcResponse = sfcResponse)
            .Select(async sfcResponse => await this._unitUnderTestRepository.SaveChangesAsync())
            .Select(async _ => await this._stopService.Calculate(CurrentUnitUnderTest))
            .Concat()
            .Do(this.ShowResult)
            .Do(this.MoveFilesToBackup)
            .Subscribe();

        this._disposables.Add(uutSenderServiceIsRunningChangeDisposable);
        this._disposables.Add(uutSenderServiceStateChangeDisposable);
        this._disposables.Add(uutCreatedDisposable);
        this._disposables.Add(uutResponseCreatedDisposable);
    }

    protected override void OnActivated()
    {
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        Messenger.Register<WaitForDummyMessage>(this, this.OnWaitForDummyMessage);
        base.OnActivated();
    }

    private void OnWaitForDummyMessage(object recipient, WaitForDummyMessage message)
    {
        if (this._uutSenderService != null)
        {
            this._uutSenderService.IsWaitingForDummy = message.Value;
        }
    }

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        this._disposables.Dispose();
    }

    [RelayCommand]
    private void Start()
    {
        try
        {
            if (this.IsRunning) return;
            this.SetupReactiveObservers();
            this._uutSenderService?.Start();
            this.Path = this._uutSenderService?.Path ?? "";
            this._stopService.Start();
            this.IsRunning = true;
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
            this.CurrentUnitUnderTest = UnitUnderTest.Null;
            this.Path = this._uutSenderService?.Path ?? "";
            this._disposables.Dispose();
            this._uutSenderService?.Stop();
            this._stopService.Stop();
            this.ShowInfoToast(Resources.msg_uut_processor_stopped);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    private void ShowResult(Stop stop)
    {
        if (stop.IsNull)
        {
            Messenger.Send(new ShowSuccessMessage(CurrentUnitUnderTest));
        }
        else
        {
            _session.Stop = stop;
            Messenger.Send(new ShowStopMessage(stop));
        }
    }

    private async Task MoveFilesToBackup(Stop stop)
    {
        return;
        //await this._fileService.MoveToBackupAndAppendDateToNameAsync(CurrentUnitUnderTest.FullPath);
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