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
using Avalonia;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _stateText = Resources.enum_stopped;
    [ObservableProperty] private bool _isWaitingForDummy;
    [ObservableProperty] private bool _isOptionVisible;
    //[ObservableProperty] private string _bakeRealTemp = "-";
    //public BindableReactiveProperty<float> BakeRealTemp { get; }
    //partial void OnBakeRealTempChanging(string? value)
    //{
    //    Debug.WriteLine($"BakeRealTemp is about to change to {value}");
    //}

    //partial void OnBakeRealTempChanged(string? value)
    //{
    //    Debug.WriteLine($"BakeRealTemp has changed to {value}");
    //}
    //public ReactiveProperty<UnitUnderTest> CurrentUnitUnderTest { get; } = new(Models.UnitUnderTest.Null);
    public ScannerViewModel ScannerViewModel { get; }
    public DummyViewModel DummyViewModel { get; }
    public ConciseMainViewModel ConciseMainViewModel { get; }
    [ObservableProperty]
    public Device _deviceModel;
    private readonly IServiceProvider _serviceProvider;
    //private readonly FileService _fileService;
    //private readonly ILogger _logger;
    //private readonly Session _session;
    //private readonly StopService _stopService;
    //private readonly UnitUnderTestRepository _unitUnderTestRepository;
    //private readonly UutSenderService _uutSenderService;

    public UutProcessorViewModel(
        ILogger logger,
        //Device device,
        IServiceProvider serviceProvider,
        //Session session,
        Settings settings,
        //StopService stopService,
        //FileService fileService,
        UutSenderServiceFactory uutSenderServiceFactory,
        UnitUnderTestRepository unitUnderTestRepository,
        ScannerViewModel scannerViewModel,
        DummyViewModel dummyViewModel,
        ConciseMainViewModel conciseMainViewModel)
        : base(
            "Ö÷½çÃæ",
            MaterialIconKind.FolderEye,
            1)
    {
        this.DummyViewModel = dummyViewModel;
        this.ScannerViewModel = scannerViewModel;
        this.ConciseMainViewModel = conciseMainViewModel;
        //this.DeviceModel = device;
        //this._fileService = fileService;
        //this._logger = logger;
        //this._session = session;
        //this._stopService = stopService;
        //this._unitUnderTestRepository = unitUnderTestRepository;
        //this._uutSenderService = uutSenderServiceFactory.Build();
        //this.CurrentUnitUnderTest = this._uutSenderService
        //    .UnitUnderTest
        //    .ToBindableReactiveProperty<UnitUnderTest>();
        this.SetupReactiveExtensionsOnActivation = true;
        this.IsActive = true;
        //this.BakeRealTemp = this.DeviceModel.BakeRealTemp.ToBindableReactiveProperty();
        this._serviceProvider = serviceProvider;
        var device = this._serviceProvider.GetRequiredService<Device>();
        this.DeviceModel = device;
        //if (settings.AutostartUutProcessor)
        //{
        //    this.Start();
        //}
    }

    protected override void SetupReactiveExtensions()
    {
        //this.DeviceModel
        //    .BakeRealTemp
        //    .Subscribe(x => this.BakeRealTemp = x.ToString("F1"))
        //    .AddTo(ref Disposables);

        //this._uutSenderService
        //    .State
        //    .Do(x => this._session.UutProcessorState.Value = x)
        //    .Do(x => this.StateText = x.ToTranslatedString())
        //    .SkipWhile(x => x == StateType.Stopped)
        //    .Do(x =>
        //    {
        //        if (x == StateType.Stopped)
        //        {
        //            this.Stop();
        //        }
        //    })
        //    .Subscribe()
        //    .AddTo(ref Disposables);

        //this._uutSenderService
        //    .UnitUnderTest
        //    .Where(unitUnderTest => !unitUnderTest.IsNull)
        //    .SelectAwait(async (unitUnderTest, _) =>
        //        (stop: await this._stopService.Calculate(unitUnderTest), unitUnderTest))
        //    .Do(x => this.ShowResult(x.stop, x.unitUnderTest))
        //    .SubscribeAwait(async (x, _) =>
        //    {
        //        x.unitUnderTest.Stop = x.stop.IsNull ? null : x.stop;
        //        await this.MoveFilesToBackup(x.unitUnderTest);
        //        await this.Persist(x.unitUnderTest);
        //    })
        //    .AddTo(ref Disposables);
    }

    protected override void OnActivated()
    {
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        //Messenger.Register<WaitForDummyMessage>(this, this.OnWaitForDummyMessage);
        //Messenger.Register<HeartBeatMessage>(this, this.OnHeartBeatMessage);
        Messenger.Register<ReSendUnitUnderTestMessage>(this, this.OnReSendUnitUnderTestMessage);
        base.OnActivated();
    }

    private void OnReSendUnitUnderTestMessage(object recipient, ReSendUnitUnderTestMessage message)
    {
        //if (this._uutSenderService.CanReSend(message.Value))
        //{
        //    this._uutSenderService.ReSend(message.Value);
        //}
        //else
        //{
        //    this.ShowErrorToast(Resources.msg_can_not_resed_uut);
        //}
    }

    //private void OnHeartBeatMessage(object recipient, HeartBeatMessage message) => this.BakeRealTemp = DeviceModel.BakeRealTemp.ToString();

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        base.OnDeactivated();
    }

    [RelayCommand]
    private void Start()
    {
        //this.BakeRealTemp = "2.2";
        var device = this._serviceProvider.GetRequiredService<Device>();
        Debug.WriteLine(ReferenceEquals(device, this.DeviceModel));
        var device2 = this._serviceProvider.GetRequiredService<Device>();
        Debug.WriteLine(ReferenceEquals(device, device2));
        Debug.WriteLine("Into Start Command");
    }

    [RelayCommand]
    private void Stop()
    {
        //try
        //{
        //    if (!this.IsRunning) return;
        //    this.IsRunning = false;
        //    this.Path = this._uutSenderService.Path;
        //    this._uutSenderService.Stop();
        //    this._stopService.Stop();
        //    this.Disposables.Clear();
        //    this.ShowInfoToast(Resources.msg_uut_processor_stopped);
        //}
        //catch (Exception e)
        //{
        //    _logger.Error(e.Message);
        //    this.ShowErrorToast(e.Message);
        //}
    }

    //[RelayCommand]
    //private void ToggleBorderClick()
    //{
    //    SetupBorderClickObservable();
    //}

    //private void SetupBorderClickObservable()
    //{

    //}

    //private void ShowResult(Stop stop, UnitUnderTest unitUnderTest)
    //{
    //    if (stop.IsNull)
    //    {
    //        Messenger.Send(new ShowSuccessMessage(unitUnderTest));
    //    }
    //    else
    //    {
    //        _session.Stop = stop;
    //        Messenger.Send(new ShowStopMessage(stop));
    //    }
    //}

    //private async Task MoveFilesToBackup(UnitUnderTest unitUnderTest)
    //{
    //    unitUnderTest.FullPath = await this._fileService
    //        .MoveToBackupAsync(unitUnderTest.FullPath);
    //    unitUnderTest.SfcResponseFullPath = await this._fileService
    //        .MoveToBackupAsync(unitUnderTest.SfcResponseFullPath);
    //}

    //private async Task Persist(UnitUnderTest unitUnderTest)
    //{
    //    await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
    //}

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }
}