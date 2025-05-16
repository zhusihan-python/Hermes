using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using Hermes.Common.Messages;
using Hermes.Communication.SerialPort;
using Hermes.Models;
using Hermes.Types;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tmds.DBus.Protocol;

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
    //[ObservableProperty] private string _taskHeader;

    private ObservableCollection<string> _alarmMessages;
    public ObservableCollection<string> AlarmMessages
    {
        get => _alarmMessages;
        set => SetProperty(ref _alarmMessages, value);
    }
    private ObservableCollection<DistributedTask> _sentTasks;
    public ObservableCollection<DistributedTask> SentTasks
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
        this._sentTasks = new ObservableCollection<DistributedTask>();
        this.SentTasks.Add(new DistributedTask("", 0, "空闲"));
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
        Messenger.Register<AddTaskMessage>(this, this.AddTask);
    }

    protected override void SetupReactiveExtensions()
    {

    }

    //private void UpdateTaskHeader()
    //{
    //    TaskHeader = _sentTasks.Count > 0 ? "待执行任务" : "空闲中";
    //}

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
            _ = Task.Run(() => this._sender.EnqueueMessage(packet));
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

    private void AddTask(object? recipient, AddTaskMessage message)
    {
        if (message != null)
        {
            var task = message.Value;
            SentTasks.Add(task);
        }
    }
}