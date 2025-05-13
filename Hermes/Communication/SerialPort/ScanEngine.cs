using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
// using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using Hermes.Common.Messages;
using Hermes.Communication.Protocol;
using LanguageExt;
using RJCP.IO.Ports;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;


public class ScanEngine : ObservableRecipient
{
    private SerialPortStream _serialPort;
    public bool IsOpen => _serialPort.IsOpen;
    private List<byte> _receiveBuffer = new List<byte>();
    private const int ReadBufferSize = 1024; // 每次读取的缓冲区大小
    private const int MaxBufferSize = 4096; // 最大接收缓冲区大小
    private readonly ILogger _logger;
    public event ReceiveDataEventHandler ReceiveDataEvent;
    private readonly ConcurrentQueue<PacketResult> _resultQueue;
    public event EventHandler<byte[]> ScanMessageReceived;
    public event EventHandler<byte[]> ScanFailed;

    public ScanEngine(
        ILogger logger
        )
    {
        _serialPort = new SerialPortStream();
        _resultQueue = new ConcurrentQueue<PacketResult>();
        _logger = logger;
    }

    public void SetSerialPort(string portName, int baudrate)
    {
        //端口名
        _serialPort.PortName = portName;

        //波特率
        _serialPort.BaudRate = baudrate;

        //奇偶校验
        _serialPort.Parity = Parity.None;

        //数据位
        _serialPort.DataBits = 8;

        //停止位
        _serialPort.StopBits = StopBits.One;
        _serialPort.NewLine = "\r";
        //串口接收数据事件
        _serialPort.DataReceived += ReceiveDataMethod;
    }

    public void Open()
    {
        //打开串口
        try
        {
            _serialPort.Open();
            _logger.Info($"扫描串口 {_serialPort.PortName} 连接成功");
        }
        catch (Exception)
        {
            //MessageBox.Show("串口被占用");
        }

    }

    public void InitialScanner()
    {
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    public void Close()
    {
        _serialPort.Close();
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="data">要发送的数据</param>
    public void SendDataMethod(byte[] data)
    {
        //获取串口状态，true为已打开，false为未打开
        bool isOpen = _serialPort.IsOpen;

        if (!isOpen)
        {
            Open();
        }

        //发送字节数组
        //参数1：包含要写入端口的数据的字节数组。
        //参数2：参数中从零开始的字节偏移量，从此处开始将字节复制到端口。
        //参数3：要写入的字节数。 
        _serialPort.Write(data, 0, data.Length);
    }
    /// <summary>
    /// 串口接收到数据触发此方法进行数据读取
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReceiveDataMethod(object sender, SerialDataReceivedEventArgs e)
    {
        int bytesToRead = _serialPort.BytesToRead;
        if (bytesToRead > 0)
        {
            byte[] buffer = new byte[bytesToRead];
            try
            {
                var bytesRead = _serialPort.Read(buffer, 0, bytesToRead);
                if (bytesRead > 0)
                {
                    _receiveBuffer.AddRange(buffer);
                    ProcessReceivedData();
                }
            }
            catch (Exception ex)
            {
                _logger.Info($"读取串口数据出错: {ex.Message}");
                _receiveBuffer.Clear(); // 发生错误时清理缓冲区
            }
        }
    }

    private void ProcessReceivedData()
    {
        _logger.Info($"_receiveBuffer {string.Join(" ", _receiveBuffer.Select(b => b.ToString("X2")))}");
        while (true)
        {
            int endIndex = IndexOf(_receiveBuffer, Scan.FullTail);
            _logger.Info($"ProcessReceivedBuffer endIndex {endIndex}");
            if (endIndex != -1)
            {
                byte[] messageData = _receiveBuffer.Take(endIndex + Scan.FullTail.Length).ToArray();
                _receiveBuffer.RemoveRange(0, endIndex + Scan.FullTail.Length);
                if (messageData.Length > 0)
                {
                    _logger.Info($"扫描串口消息={string.Join(" ", messageData.Select(b => b.ToString("X2")))}");

                    _resultQueue.TryDequeue(out var item);
                    // 判断是否超时 超时直接丢弃
                    var request = BuildRequestInfo(messageData);

                    if (item is not null && IsWithinTwoSeconds(item.Timestamp))
                    {
                        _logger.Info($"TryDequeue item {item.SlideSeq}");
                        request.Match(
                            Right: requestInfo =>
                            {
                                _logger.Info($"Scan Request Info: {string.Join(" ", requestInfo.dataFrame.Select(b => b.ToString("X2")))}");
                                Messenger.Send(new SlideInfoMessage((item.SlideSeq, requestInfo.dataFrame)));
                                ScanMessageReceived?.Invoke(this, item.FrameNo!);
                            },
                            Left: error =>
                            {
                                _logger.Info($"Error building scan request info: {error.Message}");
                            });
                    }
                    else
                    {
                        request.Match(
                            Right: requestInfo =>
                            {
                                _logger.Info($"超时：来自{requestInfo.dataFrame}的消息已处理。");
                                ScanFailed?.Invoke(this, requestInfo.dataFrame);
                            },
                            Left: error =>
                            {
                                _logger.Info($"Error building scan request info: {error.Message}");
                            });
                    }
                }
            }
            else
            {
                break; // 没有找到结束符，等待更多数据
            }

            if (_receiveBuffer.Count > MaxBufferSize)
            {
                _logger.Info("接收缓冲区过大，停止处理。");
                _receiveBuffer.Clear();
            }
        }
    }
    
    private int IndexOf(List<byte> buffer, byte[] pattern)
    {
        for (int i = 0; i <= buffer.Count - pattern.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                return i;
            }
        }
        return -1;
    }
    
    // public async Task SendDataMethodAsync(byte[] data)
    // {
    //     bool isOpen = _serialPort.IsOpen;
    //
    //     if (!isOpen)
    //     {
    //         Open();
    //         if (!_serialPort.IsOpen)
    //         {
    //             _logger.Info($"串口 {_serialPort.PortName} 未打开，无法发送数据。");
    //             return;
    //         }
    //     }
    //
    //     try
    //     {
    //         await _serialPort.WriteAsync(data, 0, data.Length);
    //         _logger.Info($"发送数据: {BitConverter.ToString(data).Replace('-', ' ')}");
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         _logger.Info("发送数据操作被取消。");
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.Info($"发送数据失败: {ex.Message}");
    //     }
    // }

    private static Either<Exception, ScanRequestInfo> BuildRequestInfo(byte[] data)
    {
        if (data == null || data.Length < Scan.MiniLength)
        {
            return Either<Exception, ScanRequestInfo>.Left(new ArgumentException($"Data length less than minimum required length: {Scan.MiniLength}"));
        }
        ReadOnlySpan<byte> span = data.AsSpan();
        var tailIndex = span.IndexOfFirst(0, span.Length, Scan.FullTail);
        if (tailIndex > 0)
        {
            // 从包尾往前找包头
            var headIndexes = span.IndexOfInclude(0, tailIndex - Scan.FullTail.Length, Scan.FullHead);
            // 找到取第一个
            if (headIndexes.Count > 0)
            {
                // 取距离包尾最近的包头
                var lastHeadIndex = headIndexes[^1];
                var startIndex = lastHeadIndex + 1;
                // var length = tailIndex + 1 - tailLength - startIndex;
                var length = tailIndex - startIndex;
                var package = span.Slice(startIndex, length).ToArray();
                // contain barcode length bigger than 2
                if (package.Length > 2)
                {
                    var requestInfo = new ScanRequestInfo();
                    requestInfo.dataFrame = package;
                    return Either<Exception, ScanRequestInfo>.Right(requestInfo);
                }
                else
                {
                    return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("Scan Data length Check Failed"));
                }
            }
            else
            {
                return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("Scan No Head Found"));
            }
        }
        else
        {
            return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("Scan No Tail Found"));
        }
    }

    public async Task SendPacket(ScanRequestInfo packet)
    {
        await Task.Delay(100);
        _logger.Info($"Send ScanRequestInfo: {string.Join(" ", packet.dataFrame.Select(b => b.ToString("X2")))}");
        var packetResult = new PacketResult { SlideSeq = packet.SlideSeq, FrameNo = packet.FrameNo };
        _resultQueue.Enqueue(packetResult);
        // 启动超时检查
        SendDataMethod(packet.dataFrame);
        await StartTimeoutCheckAsync(packet.FrameNo);
    }

    private async Task StartTimeoutCheckAsync(byte[] frameNo)
    {
        await Task.Delay(3000); // 3秒超时
        // 超时后尝试移除未处理的请求
        RemoveFromQueue(frameNo);
    }

    private void RemoveFromQueue(byte[] frameNo)
    {
        _logger.Info($"into RemoveFromQueue Queue Count {_resultQueue.Count}");
        _resultQueue.TryDequeue(out var item);
        if (item != null && Enumerable.SequenceEqual(item.FrameNo!, frameNo))
        {
            _logger.Info($"移除请求 (frameNo: {string.Join(" ", frameNo.Select(b => b.ToString("X2")))})");
            _logger.Info($"移除请求 (item SlideSeq: {item.SlideSeq})");
            _logger.Info($"移除请求 (item FrameNo: {string.Join(" ", item.FrameNo.Select(b => b.ToString("X2")))})");
        }
    }

    public static bool IsWithinTwoSeconds(DateTime timeStamp)
    {
        DateTime now = DateTime.Now;
        TimeSpan difference = now - timeStamp;
        return Math.Abs(difference.TotalSeconds) <= 3;
    }
}

public class PacketResult
{
    public ushort SlideSeq;
    public byte[]? FrameNo { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}