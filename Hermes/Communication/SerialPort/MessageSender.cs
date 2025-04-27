using System;
using System.Diagnostics;
//using System.Linq;
//using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Communication.SerialPort;

public class MessageSender : IDisposable
{
    //private ConcurrentQueue<SvtRequestInfo> _messageQueue = new();
    private readonly Timer _timer;
    private ComPort _comPort;
    public ScanEngine scanEngine;
    //private readonly ManualResetEventSlim _filterSignal = new(true); // 初始化为允许SendMessageAsync运行
    //private bool _timerRunning; // 添加一个标志来跟踪定时器状态
    private bool _disposed;

    public MessageSender(ComPort comPort, ScanEngine scanEngine)
    {
        //_messageQueue = new ConcurrentQueue<SvtRequestInfo>();
        this._comPort = comPort;
        this.scanEngine = scanEngine;
        //_timer = new Timer(async state => await SendMessageAsync(), null, Timeout.Infinite, Timeout.Infinite);
        //_timerRunning = false; // 初始化标志
        this.scanEngine.ScanMessageReceived += OnScanMessageReceived;
        this.scanEngine.ScanFailed += OnScanFailed;
    }

    public void InitializeComPort()
    {
        //await _comPort.InitializeAsync("COM3", 115200);
        _comPort.SetSerialPort("COM3", 115200);
        _comPort.Open();
        scanEngine.SetSerialPort("COM4", 9600);
        scanEngine.Open();
        //await scanEngine.InitializeAsync("COM4", 9600);
    }

    public async Task EnqueueMessage(SvtRequestInfo message)
    {
        await _comPort.SendPacket(message);
    }

    //public void EnqueueMessageArray(SvtRequestInfo[] messages)
    //{
    //    if (messages == null || messages.Length == 0)
    //    {
    //        return;
    //    }
    //    messages.ForEach(message => _messageQueue.Enqueue(message));
    //}

    //private void TryStartTimer()
    //{
    //    if (!_timerRunning && _comPort.ClientOnline && !_messageQueue.IsEmpty)
    //    {
    //        _timer.Change(200, Timeout.Infinite); // 单次触发，避免并发
    //        _timerRunning = true;
    //    }
    //}

    //public void StopTimer()
    //{
    //    _timer.Change(Timeout.Infinite, Timeout.Infinite); // 停止定时器
    //    _timerRunning = false;
    //}

    //public void ApplyFilter(Func<SvtRequestInfo, bool> filter)
    //{
    //    if (filter == null) throw new ArgumentNullException(nameof(filter));

    //    _filterSignal.Reset(); // 阻止 SendMessageAsync 运行
    //    _messageQueue = new ConcurrentQueue<SvtRequestInfo>(_messageQueue.Where(filter));
    //    _filterSignal.Set(); // 允许 SendMessageAsync 运行
    //}

    //private async Task SendMessageAsync()
    //{
    //    _filterSignal.Wait(); // 等待过滤信号
    //    // 如果队列为空，则停止定时器
    //    if (!_comPort.ClientOnline || _messageQueue.IsEmpty)
    //    {
    //        StopTimer();
    //        return;
    //    }

    //    if (_messageQueue.TryDequeue(out var message))
    //    {
    //        try
    //        {
    //            _comPort.SendPacket(message);
    //            await Task.Delay(50);
    //        }
    //        catch (Exception ex)
    //        {
    //            // 处理发送消息时的异常，例如记录日志
    //            Console.WriteLine($"发送消息失败：{ex.Message}");
    //        }
    //    }

    //    // 继续处理下一条
    //    if (!_comPort.ClientOnline || _messageQueue.IsEmpty)
    //    {
    //        _timer.Change(200, Timeout.Infinite);
    //    }
    //    else
    //    {
    //        StopTimer();
    //    }
    //}

    public async Task SendScannerMessageAsync(ScanRequestInfo message)
    {
        await scanEngine.SendPacket(message);
    }

    private void OnScanMessageReceived(object? sender, byte[] frameNumber)
    {
        Debug.WriteLine("扫描成功");
        var request = new ScanResultWrite(frameNumber).ScanSuccess();
        _ = Task.Run(() => EnqueueMessage(request));
    }

    private void OnScanFailed(object? sender, byte[] frameNumber)
    {
        Debug.WriteLine("扫描失败");
        var request = new ScanResultWrite(frameNumber).ScanFail();
        _ = Task.Run(() => EnqueueMessage(request));
    }

    public bool GetClientState()
    {
        return _comPort.ClientOnline;
    }

    public bool GetScannerState()
    {
        return scanEngine.ClientOnline;
    }

    public void Dispose()
    {
        if (_disposed) return;

        this.scanEngine.ScanMessageReceived -= OnScanMessageReceived;
        this.scanEngine.ScanFailed -= OnScanFailed; // 取消订阅 ScanFailed 事件
        _timer.Dispose();
        _disposed = true;
    }
}