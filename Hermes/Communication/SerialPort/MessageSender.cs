using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class MessageSender : IDisposable
{
    private ConcurrentQueue<SvtRequestInfo> _messageQueue = new();
    private readonly Timer _timer;
    private ComPort _comPort;
    private ScanEngine scanEngine;
    private readonly ManualResetEventSlim _filterSignal = new(true); // 初始化为允许SendMessageAsync运行
    private bool _timerRunning; // 添加一个标志来跟踪定时器状态
    private bool _disposed;

    public MessageSender(ComPort comPort, ScanEngine scanEngine)
    {
        //_messageQueue = new ConcurrentQueue<SvtRequestInfo>();
        this._comPort = comPort;
        this.scanEngine = scanEngine;
        _timer = new Timer(async state => await SendMessageAsync(), null, Timeout.Infinite, Timeout.Infinite);
        _timerRunning = false; // 初始化标志
    }

    public async void InitializeComPort()
    {
        await _comPort.InitializeAsync("COM1", 115200);
        await scanEngine.InitializeAsync("COM7", 9600);
    }

    public void EnqueueMessage(SvtRequestInfo message)
    {
        _messageQueue.Enqueue(message);
        // 如果队列中有消息，并且定时器未启动，则启动定时器
        TryStartTimer();
    }

    public void EnqueueMessageArray(SvtRequestInfo[] messages)
    {
        if (messages == null || messages.Length == 0)
        {
            return;
        }
        messages.ForEach(message => _messageQueue.Enqueue(message));
    }

    private void TryStartTimer()
    {
        if (!_timerRunning && _comPort.ClientOnline && !_messageQueue.IsEmpty)
        {
            _timer.Change(200, Timeout.Infinite); // 单次触发，避免并发
            _timerRunning = true;
        }
    }

    public void StopTimer()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite); // 停止定时器
        _timerRunning = false;
    }

    public void ApplyFilter(Func<SvtRequestInfo, bool> filter)
    {
        if (filter == null) throw new ArgumentNullException(nameof(filter));

        _filterSignal.Reset(); // 阻止 SendMessageAsync 运行
        _messageQueue = new ConcurrentQueue<SvtRequestInfo>(_messageQueue.Where(filter));
        _filterSignal.Set(); // 允许 SendMessageAsync 运行
    }

    private async Task SendMessageAsync()
    {
        _filterSignal.Wait(); // 等待过滤信号
        // 如果队列为空，则停止定时器
        if (!_comPort.ClientOnline || _messageQueue.IsEmpty)
        {
            StopTimer();
            return;
        }

        if (_messageQueue.TryDequeue(out var message))
        {
            try
            {
                await _comPort.SendPacketAsync(message);
            }
            catch (Exception ex)
            {
                // 处理发送消息时的异常，例如记录日志
                Console.WriteLine($"发送消息失败：{ex.Message}");
            }
        }

        // 继续处理下一条
        if (!_comPort.ClientOnline || _messageQueue.IsEmpty)
        {
            _timer.Change(200, Timeout.Infinite);
        }
        else
        {
            StopTimer();
        }
    }

    public async Task SendScannerMessageAsync(ScanRequestInfo message)
    {
        await scanEngine.SendPacketAsync(message);
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

        _timer.Dispose();
        _disposed = true;
    }
}