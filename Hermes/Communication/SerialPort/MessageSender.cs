using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Communication.SerialPort;

public class MessageSender : IDisposable
{
    private ConcurrentQueue<SvtRequestInfo> _messageQueue = new();
    private readonly Timer _timer;
    private ComPort _comPort;
    public ScanEngine scanEngine;
    private const int HeartbeatIntervalMs = 2000;
    //private readonly ManualResetEventSlim _filterSignal = new(true); // 初始化为允许SendMessageAsync运行
    //private bool _timerRunning; // 添加一个标志来跟踪定时器状态
    private bool _disposed;

    public MessageSender(ComPort comPort, ScanEngine scanEngine)
    {
        //_messageQueue = new ConcurrentQueue<SvtRequestInfo>();
        this._comPort = comPort;
        this.scanEngine = scanEngine;
        this._timer = new Timer(async state => await SendMessageAsync(), null, Timeout.Infinite, 100);
        //_timer = new Timer(async state => await SendMessageAsync(), null, Timeout.Infinite, Timeout.Infinite);
        //_timerRunning = false; // 初始化标志
        this.scanEngine.ScanMessageReceived += OnScanMessageReceived;
        this.scanEngine.ScanFailed += OnScanFailed;
    }

    public void InitializeComPort()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _comPort.SetSerialPort("COM3", 115200);
            scanEngine.SetSerialPort("COM4", 9600);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _comPort.SetSerialPort("/dev/ttyUSB1", 115200);
            scanEngine.SetSerialPort("/dev/ttyUSB0", 9600);
        }
        try
        {
            if (!_comPort.IsOpen)
            {
                _comPort.Open();
                _timer.Change(0, 100);
                if (_comPort.IsOpen)
                {
                    _ = Task.Run(HeartbeatLoop);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"打开 ComPort 失败: {ex.Message}");
        }

        try
        {
            if (!scanEngine.IsOpen)
            {
                scanEngine.Open();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"打开 ScanEngine 失败: {ex.Message}");
        }
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

    private async Task SendMessageAsync()
    {
        // 如果队列为空，则停止定时器
        if (!_comPort.IsOpen || _messageQueue.IsEmpty)
        {
            return;
        }

        if (_messageQueue.TryDequeue(out var message))
        {
            try
            {
                await _comPort.SendPacket(message);
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                // 处理发送消息时的异常，例如记录日志
                Console.WriteLine($"发送消息失败：{ex.Message}");
            }
        }

        // 继续处理下一条
        //if (!_comPort.IsOpen || _messageQueue.IsEmpty)
        //{
        //    _timer.Change(200, Timeout.Infinite);
        //}
        //else
        //{
        //    StopTimer();
        //}
    }

    public async Task SendScannerMessageAsync(ScanRequestInfo message)
    {
        await scanEngine.SendPacket(message);
    }

    private async void OnScanMessageReceived(object? sender, byte[] frameNumber)
    {
        Debug.WriteLine("扫描成功");
        var request = new ScanResultWrite(frameNumber).ScanSuccess();
        await EnqueueMessage(request);
    }

    private async void OnScanFailed(object? sender, byte[] frameNumber)
    {
        Debug.WriteLine("扫描失败");
        var request = new ScanResultWrite(frameNumber).ScanFail();
        await EnqueueMessage(request);
    }

    public bool GetClientState()
    {
        return _comPort.IsOpen;
    }

    public bool GetScannerState()
    {
        return scanEngine.IsOpen;
    }

    private async Task HeartbeatLoop()
    {
        while (this._comPort.IsOpen)
        {
            var heartbeat = new HeartBeatRead();
            await EnqueueMessage(heartbeat);
            try
            {
                await Task.Delay(HeartbeatIntervalMs);
            }
            catch (TaskCanceledException)
            {
                // Task.Delay 可能会在程序关闭时抛出这个异常
                Debug.WriteLine("心跳循环中的延迟被取消。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"心跳循环发生异常: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5)); // 发生错误后等待一段时间再重试
            }
        }
        Debug.WriteLine("心跳循环已停止（连接断开）。");
    }

    public void Dispose()
    {
        if (_disposed) return;

        this.scanEngine.ScanMessageReceived -= OnScanMessageReceived;
        this.scanEngine.ScanFailed -= OnScanFailed; // 取消订阅 ScanFailed 事件
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
        _disposed = true;
    }
}