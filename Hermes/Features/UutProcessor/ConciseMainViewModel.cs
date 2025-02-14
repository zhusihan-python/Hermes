using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common;
using System.Threading.Tasks;
using System;
using R3;
using Hermes.Communication.SerialPort;
using System.Windows.Input;

namespace Hermes.Features.UutProcessor;

public partial class ConciseMainViewModel : ViewModelBase
{
    public ReactiveProperty<string> ScannedText { get; }
    public ReactiveProperty<bool> State { get; }

    [ObservableProperty] private bool _isConnected;

    private readonly ILogger _logger;
    //private ComPort _comPort;
    //private ComPort ComPort { get => this._comPort; set => _comPort = value; }
    private readonly MessageSender _sender;
    public ICommand SealSlideCommand { get; }
    public ICommand SortSlideCommand { get; }


    public ConciseMainViewModel(
        ILogger logger,
        MessageSender sender)
    {
        this._logger = logger;
        this._sender = sender;
        this.State = new ReactiveProperty<bool>(_sender.GetClientState());
        SealSlideCommand = new AsyncRelayCommand(SealSlide);
        SortSlideCommand = new AsyncRelayCommand(SortSlide);
    }

    protected override void SetupReactiveExtensions()
    {

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
            var packet = new HeartBeatRead();
            this._sender.EnqueueMessage(packet);
            //this.ComPort.EnqueuePacket(packet);
            //await this.ComPort.SendPacketAsync(packet);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }
}