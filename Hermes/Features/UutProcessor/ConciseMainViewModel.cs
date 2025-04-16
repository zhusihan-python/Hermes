using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using System.Threading.Tasks;
using System;
using R3;
using Hermes.Communication.SerialPort;
using System.Windows.Input;
using Hermes.Common.Messages;
using Hermes.Models;

namespace Hermes.Features.UutProcessor;

public partial class ConciseMainViewModel : ViewModelBase
{
    public ReactiveProperty<string> ScannedText { get; }
    public ReactiveProperty<bool> State { get; }

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _currentDay;
    [ObservableProperty] private string _currentHour;
    [ObservableProperty] private ushort _leftCovers;

    private readonly ILogger _logger;
    private readonly Device _device;
    private readonly MessageSender _sender;

    public ICommand SealSlideCommand { get; }
    public ICommand SortSlideCommand { get; }


    public ConciseMainViewModel(
        ILogger logger,
        Device device,
        MessageSender sender)
    {
        this._logger = logger;
        this._device = device;
        this._sender = sender;
        this.State = new ReactiveProperty<bool>(_sender.GetClientState());
        SealSlideCommand = new AsyncRelayCommand(SealSlide);
        SortSlideCommand = new AsyncRelayCommand(SortSlide);
        Messenger.Register<HeartBeatMessage>(this, this.Refresh);
        // 初次赋值
        var curDateTime = DateTime.Now;
        CurrentDay = curDateTime.ToString("MM-dd yyyy");
        CurrentHour = curDateTime.ToString("HH:mm");
        // 每分钟更新一次（如果需要实时更新）
        Observable.Interval(TimeSpan.FromMinutes(1))
                  .Subscribe(_ =>
                  {
                      var curDateTime = DateTime.Now;
                      CurrentDay = curDateTime.ToString("MM-dd yyyy");
                      CurrentHour = curDateTime.ToString("HH:mm");
                  });
    }

    protected override void SetupReactiveExtensions()
    {

    }

    public void Refresh(object? recipient, HeartBeatMessage message)
    {
        if (this._device == null)
        {
            return;
        }
        LeftCovers = this._device.CoverBoxLeftCount;
    }

    private async Task SealSlide()
    {
        try
        {
            var boxTags = new byte[75];
            boxTags[21] = 0x01;
            var packet = new SystemStatusWrite().
                            WithOperationType(0x04).
                            WithMasterAddress<SystemStatusWrite>(0xF2).
                            WithSlaveAddress<SystemStatusWrite>(0x13).
                            WithBoxTags(boxTags);
            //this.ComPort.EnqueuePacket(packet);
            //await this.ComPort.SendPacketAsync(packet);
            this._sender.EnqueueMessage(packet);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    private async Task SortSlide()
    {
        try
        {
            //var packet = new HeartBeatRead();
            //this._sender.EnqueueMessage(packet);

            var frameNumber = new byte[] { 0x00, 0x01 };
            var scanRequest = new ScanStartRequest(0x0001, frameNumber);
            await this._sender.SendScannerMessageAsync(scanRequest);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }
}