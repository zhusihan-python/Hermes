using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services.UutSenderService;
using Material.Icons;
using System.Linq;
using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Collections;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    public AvaloniaList<string> SortOptions { get; } = [];
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _stateText = Resources.enum_stopped;
    [ObservableProperty] private bool _isWaitingForDummy;
    [ObservableProperty] private bool _isOptionVisible;
    [ObservableProperty] private string _selectedSortOption;

    public ScannerViewModel ScannerViewModel { get; }
    public DummyViewModel DummyViewModel { get; }
    public ConciseMainViewModel ConciseMainViewModel { get; }
    [ObservableProperty]
    public Device _deviceModel;
    private readonly IServiceProvider _serviceProvider;

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
            "主界面",
            MaterialIconKind.FolderEye,
            1)
    {
        this.DummyViewModel = dummyViewModel;
        this.ScannerViewModel = scannerViewModel;
        this.ConciseMainViewModel = conciseMainViewModel;
        this.SetupReactiveExtensionsOnActivation = true;
        this.IsActive = true;
        this._serviceProvider = serviceProvider;
        var device = this._serviceProvider.GetRequiredService<Device>();
        this.DeviceModel = device;
        SortOptions.Add("按项目");
        SortOptions.Add("按医生");
        SortOptions.Add("按病理号");
        SortOptions.Add("按玻片号");
        SelectedSortOption = SortOptions.First();
    }

    protected override void SetupReactiveExtensions()
    {

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

    [RelayCommand]
    private void SealSlide()
    {
        Messenger.Send(new SealSlideMessage());
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }
}