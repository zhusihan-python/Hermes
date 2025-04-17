using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using System.Threading.Tasks;
using System;
using R3;
using Hermes.Communication.SerialPort;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Types;
using System.Security.Claims;

namespace Hermes.Features.UutProcessor;

public partial class ConciseMainViewModel : ViewModelBase
{
    public ReactiveProperty<string> ScannedText { get; }
    public ReactiveProperty<bool> State { get; }

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _currentDay;
    [ObservableProperty] private string _currentHour;
    [ObservableProperty] private string _currentTimeStamp;
    [ObservableProperty] private ushort _leftCovers;

    private ObservableCollection<string> _alarmMessages;
    public ObservableCollection<string> AlarmMessages
    {
        get => _alarmMessages;
        set => SetProperty(ref _alarmMessages, value);
    }
    private ObservableCollection<DistributedTasks> _sentTasks;
    public ObservableCollection<DistributedTasks> SentTasks
    {
        get => _sentTasks;
        set => SetProperty(ref _sentTasks, value);
    }
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
        this.AlarmMessages = new ObservableCollection<string>();
        this.SentTasks = new ObservableCollection<DistributedTasks>();
        this.SentTasks.Add(new DistributedTasks("A1", 80, "封片"));
        this.SentTasks.Add(new DistributedTasks("A2", 70, "封片"));
        this.SentTasks.Add(new DistributedTasks("A3", 60, "封片"));
        this.SentTasks.Add(new DistributedTasks("A4", 70, "封片"));
        this.SentTasks.Add(new DistributedTasks("A5", 80, "封片"));
        this.SentTasks.Add(new DistributedTasks("A6", 70, "封片"));
        this.State = new ReactiveProperty<bool>(_sender.GetClientState());
        SealSlideCommand = new AsyncRelayCommand(SealSlide);
        SortSlideCommand = new AsyncRelayCommand(SortSlide);
        Messenger.Register<HeartBeatMessage>(this, this.Refresh);
        // 初次赋值
        var curDateTime = DateTime.Now;
        CurrentDay = curDateTime.ToString("MM-dd yyyy");
        CurrentHour = curDateTime.ToString("HH:mm");
        CurrentTimeStamp = curDateTime.ToString("yyyy MM-dd HH:mm:ss:fff");
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
        UpdateAlarmMessages(this._device.AlarmCodes);
    }

    public void UpdateAlarmMessages(ushort[] codes)
    {
        CurrentTimeStamp = DateTime.Now.ToString("yyyy MM-dd HH:mm:ss:fff");
        // 清空之前的告警消息
        _alarmMessages.Clear();

        // 使用 LINQ 查询 AlarmCodes 中非零的 code，并在 AlarmMap 中查找对应的 AlarmCodeInfoStruct
        var messages = codes
            .Where(code => code != 0 && AlarmCodes.AlarmMap.ContainsKey(code)) // 筛选非零的告警码
            .Select(code => AlarmCodes.AlarmMap.TryGetValue(code, out var info) ? info.ToString() : $"Unknown Alarm Code: {code}")
            .ToList();

        // 将查询到的告警消息添加到 AlarmMessages 集合中
        foreach (var message in messages)
        {
            _alarmMessages.Add(message);
        }
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

    public class DistributedTasks
    {
        public string Position { get; set; }
        public double ProgressValue { get; set; }
        public string ActionType { get; set; }

        public DistributedTasks(string position, double progressValue, string actionType)
        {
            Position = position;
            ProgressValue = progressValue;
            ActionType = actionType;
        }
    }
}