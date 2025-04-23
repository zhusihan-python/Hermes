using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Communication.Protocol;
using LanguageExt;
using RJCP.IO.Ports;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;


public class ScanEngine : ObservableRecipient
{
    private SerialPortStream _serialPort;
    public bool ClientOnline => _serialPort.IsOpen;
    public event ReceiveDataEventHandler ReceiveDataEvent;
    private readonly ConcurrentQueue<PacketResult> _resultQueue;
    public event EventHandler<byte[]> ScanMessageReceived;
    public event EventHandler<byte[]> ScanFailed;

    public ScanEngine()
    {
        _serialPort = new SerialPortStream();
        _resultQueue = new ConcurrentQueue<PacketResult>();
    }

    public static string[] GetPortArray()
    {
        return SerialPortStream.GetPortNames();
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

        _serialPort.ReceivedBytesThreshold = 7;
        //串口接收数据事件
        _serialPort.DataReceived += ReceiveDataMethod;
    }

    public void Open()
    {
        //打开串口
        try
        {
            _serialPort.Open();
            Debug.WriteLine($"扫描串口 {_serialPort.PortName} 连接成功");
        }
        catch (Exception)
        {
            //MessageBox.Show("串口被占用");
        }

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
    private async void ReceiveDataMethod(object sender, SerialDataReceivedEventArgs e)
    {
        ReceiveDataEventArg arg = new ReceiveDataEventArg();

        //读取串口缓冲区的字节数据
        arg.Data = new byte[_serialPort.BytesToRead];
        _serialPort.Read(arg.Data, 0, _serialPort.BytesToRead);

        Debug.WriteLine($"扫描串口消息={string.Join(" ", arg.Data.Select(b => b.ToString("X2")))}");

        _resultQueue.TryDequeue(out var item);
        // 判断是否超时 超时直接丢弃
        Debug.WriteLine($"TryDequeue item {item}");
        var request = BuildRequestInfo(arg.Data);

        if (item is not null && IsWithinTwoSeconds(item.Timestamp))
        {
            request.Match(
                Right: requestInfo =>
                {
                    if (requestInfo.dataFrame != null)
                    {
                        Console.WriteLine($"Scan Request Info: {requestInfo.dataFrame.Select(b => b.ToString("X2"))}");
                        Messenger.Send(new SlideInfoMessage((item.SlideSeq, requestInfo.dataFrame)));
                        ScanMessageReceived?.Invoke(this, item.FrameNo!);
                    }
                },
                Left: error =>
                {
                    Console.WriteLine($"Error building scan request info: {error.Message}");
                });
        }
        else
        {
            request.Match(
                Right: requestInfo =>
                {
                    Console.WriteLine($"超时：来自{requestInfo.dataFrame}的消息已处理。");
                    ScanFailed?.Invoke(this, requestInfo.dataFrame);
                },
                Left: error =>
                {
                    Console.WriteLine($"Error building scan request info: {error.Message}");
                });
        }

        //触发自定义消息接收事件，把串口数据发送出去
        if (ReceiveDataEvent != null && arg.Data.Length != 0)
        {
            ReceiveDataEvent.Invoke(null, arg);
        }
    }

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
                //var pos = byteBlock.Position;//记录初始游标位置，防止本次无法解析时，回退游标。
                // 去掉包头包尾
                var package = span.Slice(lastHeadIndex + 1, tailIndex + 1 - Scan.FullTail.Length - Scan.FullHead.Length).ToArray();
                // 清理之后包长度，不含包头包尾最少是14
                if (package.Length > 0)
                {
                    var requestInfo = new ScanRequestInfo();
                    requestInfo.dataFrame = package;
                    return Either<Exception, ScanRequestInfo>.Right(requestInfo);
                }
                else
                {
                    return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("Data length Check Failed"));
                }
            }
            else
            {
                return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("No Head Found"));
            }
        }
        else
        {
            return Either<Exception, ScanRequestInfo>.Left(new ArgumentException("No Tail Found"));
        }
    }

    public async Task SendPacket(ScanRequestInfo packet)
    {
        await Task.Delay(100);
        Debug.WriteLine($"Send ScanRequestInfo: {string.Join(" ", packet.dataFrame.Select(b => b.ToString("X2")))}");
        var packetResult = new PacketResult { SlideSeq = packet.SlideSeq, FrameNo = packet.FrameNo };
        _resultQueue.Enqueue(packetResult);
        // 启动超时检查
        await StartTimeoutCheckAsync(packet.FrameNo);

        SendDataMethod(packet.dataFrame);
    }

    private async Task StartTimeoutCheckAsync(byte[] frameNo)
    {
        await Task.Delay(2000); // 2秒超时
        // 超时后尝试移除未处理的请求
        RemoveFromQueue(frameNo);
    }

    private void RemoveFromQueue(byte[] frameNo)
    {
        Debug.WriteLine("into RemoveFromQueue");
        _resultQueue.TryDequeue(out var item);
        if (item != null && Enumerable.SequenceEqual(item.FrameNo!, frameNo))
        {
            Debug.WriteLine($"移除请求 (frameNo: {frameNo})");
            Debug.WriteLine($"移除请求 (item: {item})");
            Debug.WriteLine($"移除请求 (FrameNo: {item.FrameNo})");
        }
        else
        {

        }
    }

    public static bool IsWithinTwoSeconds(DateTime timeStamp)
    {
        DateTime now = DateTime.Now;
        TimeSpan difference = now - timeStamp;
        return Math.Abs(difference.TotalSeconds) <= 2;
    }
}

public class PacketResult
{
    public ushort SlideSeq;
    public byte[]? FrameNo { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}