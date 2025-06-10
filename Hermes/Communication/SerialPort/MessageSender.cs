using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Common;
using RJCP.IO.Ports;

namespace Hermes.Communication.SerialPort;

public class MessageSender : IDisposable
{
    private ConcurrentQueue<SvtRequestInfo> _messageQueue = new();
    private readonly Timer _timer;
    private ComPort _comPort;
    public ScanEngine scanEngine;
    private const int HeartbeatIntervalMs = 2000;
    private readonly ILogger _logger;
    private readonly FrameSequenceGenerator _frameSequenceGenerator = new FrameSequenceGenerator();
    private bool _disposed;

    public MessageSender(
        ILogger logger,
        ComPort comPort, 
        ScanEngine scanEngine
        )
    {
        this._logger = logger;
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
            var comPortName = "/dev/ttyS1";
            var scanPortName = "/dev/ttyS0";
#if DEBUG
            comPortName = "/dev/ttyUSB0";
            scanPortName = "/dev/ttyUSB1";
#endif
            _comPort.SetSerialPort(comPortName, 115200);
            scanEngine.SetSerialPort(scanPortName, 9600);
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
            _logger.Info($"打开 ComPort 失败: {ex.Message}");
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
            _logger.Info($"打开 ScanEngine 失败: {ex.Message}");
        }
    }

    public async Task EnqueueMessage(SvtRequestInfo message)
    {
        // await _comPort.SendPacket(message);
        message.FrameNo = GetFrameNumber();
        _messageQueue.Enqueue(message);
        await Task.CompletedTask;
    }

    public async Task EnqueueMessageWithFrameNo(SvtRequestInfo message)
    {
        _logger.Info($"EnqueueMessageWithFrameNo: {string.Join(" ", message.DataFrame().Select(b => b.ToString("X2")))}");
        _messageQueue.Enqueue(message);
        await Task.CompletedTask;
    }

    public byte[] GetFrameNumber()
    {
        return _frameSequenceGenerator.GenerateFrameSequence();
    }

    public string[] GetPortNames()
    {
        return SerialPortStream.GetPortNames();
    }

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
                _logger.Info($"发送mcu消息失败：{ex.Message}");
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
        _logger.Info("扫描成功");
        var request = new ScanResultWrite(frameNumber).ScanSuccess();
        await EnqueueMessage(request);
    }

    private async void OnScanFailed(object? sender, byte[] frameNumber)
    {
        _logger.Info("扫描失败");
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
                _logger.Debug("心跳循环中的延迟被取消。");
            }
            catch (Exception ex)
            {
                _logger.Debug($"心跳循环发生异常: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5)); // 发生错误后等待一段时间再重试
            }
        }
        _logger.Debug("心跳循环已停止（连接断开）。");
    }

    public void ClosePort()
    {
        _comPort.Close();
        scanEngine.Close();
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

public class FrameSequenceGenerator
{
    private int _counter = 0; // 计数器，用于生成序号
    private const int MaxSequence = 65535; // 最大序号值

    // 生成下一个序号
    public byte[] GenerateFrameSequence()
    {
        // 计算下一个序号
        _counter = (_counter % MaxSequence) + 1;

        // 将序号转换为大端序的字节数组
        byte[] sequenceBytes = BitConverter.GetBytes((ushort)_counter);
        if (BitConverter.IsLittleEndian)
        {
            System.Array.Reverse(sequenceBytes); // 如果是小端序，反转字节数组
        }

        return sequenceBytes;
    }
}