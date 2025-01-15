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
    private ComPort _comPort;
    private ComPort ComPort { get => this._comPort; set => _comPort = value; }
    public ICommand SealSlideCommand { get; }
    public ICommand SortSlideCommand { get; }


    public ConciseMainViewModel(
        ILogger logger,
        ComPort comPort)
    {
        this._logger = logger;
        this.ComPort = comPort;
        this.State = new ReactiveProperty<bool>(comPort.State);
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
                            WithMasterAddress<SystemStatusWrite>(0x12).
                            WithSlaveAddress<SystemStatusWrite>(0x11).
                            WithBoxTags(boxTags);
            this.ComPort.EnqueuePacket(packet);
            await this.ComPort.SendPacketAsync(packet);
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
            var boxTags = new byte[75];
            var packet = new SystemStatusWrite().
                            WithOperationType(0x01).
                            WithMasterAddress<SystemStatusWrite>(0x12).
                            WithSlaveAddress<SystemStatusWrite>(0x11).
                            WithBoxTags(boxTags);
            this.ComPort.EnqueuePacket(packet);
            await this.ComPort.SendPacketAsync(packet);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }
}