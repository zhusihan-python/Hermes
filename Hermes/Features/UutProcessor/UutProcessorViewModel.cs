using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
//using System.Linq;
using System;
using System.Diagnostics;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    public AvaloniaList<string> SortOptions { get; } = [];
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _stateText = Resources.enum_stopped;
    [ObservableProperty] private bool _isWaitingForDummy;
    [ObservableProperty] private bool _isOptionVisible;
    [ObservableProperty] private int _selectedIndex;

    public ConciseMainViewModel ConciseMainViewModel { get; }
    [ObservableProperty]
    public Device _deviceModel;
    private readonly IServiceProvider _serviceProvider;

    public UutProcessorViewModel(
        ILogger logger,
        IServiceProvider serviceProvider,
        ISukiDialogManager dialogManager,
        Settings settings,
        ConciseMainViewModel conciseMainViewModel)
        : base(
            "主界面",
            MaterialIconKind.FolderEye,
            1)
    {
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
        SelectedIndex = 0;
    }

    protected override void SetupReactiveExtensions()
    {

    }

    protected override void OnActivated()
    {
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        base.OnDeactivated();
    }

    [RelayCommand]
    private void Start()
    {
        var device = this._serviceProvider.GetRequiredService<Device>();
        Debug.WriteLine(ReferenceEquals(device, this.DeviceModel));
        var device2 = this._serviceProvider.GetRequiredService<Device>();
        Debug.WriteLine(ReferenceEquals(device, device2));
        Debug.WriteLine("Into Start Command");
    }

    [RelayCommand]
    private void SortSlide()
    {
        Messenger.Send(new SortSlideMessage(SelectedIndex));
    }

    [RelayCommand]
    private void SealSlide()
    {
        Messenger.Send(new SealSlideMessage());
    }

    [RelayCommand]
    private void Pause()
    {
        Messenger.Send(new PauseActionMessage());
    }

    [RelayCommand]
    private void Stop()
    {
        Messenger.Send(new StopActionMessage());
    }

    [RelayCommand]
    private void ShowDetail()
    {
        Messenger.Send(new ShowDetailMessage());
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        //this.Stop();
    }
}