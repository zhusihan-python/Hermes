using System.Threading.Tasks;
using System;
using System.Linq;
using LanguageExt;
using RJCP.IO.Ports;
using Hermes.Communication.Protocol;
using Hermes.Common;
using System.Collections.Generic;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public delegate void ReceiveDataEventHandler(object sender, ReceiveDataEventArg e);

public class ComPort
{
    private SerialPortStream _serialPort;
    private readonly FrameParser _parser;
    //private const int HeartbeatIntervalMs = 2000;
    public bool IsOpen => _serialPort.IsOpen;
    private List<byte> _receiveBuffer = new List<byte>();
    private const int ReadBufferSize = 1024; // 每次读取的缓冲区大小
    private const int MaxBufferSize = 4096; // 最大接收缓冲区大小
    private readonly ILogger _logger;
    public event ReceiveDataEventHandler ReceiveDataEvent;

    public ComPort(
        ILogger logger,
        FrameParser parser)
    {
        _serialPort = new SerialPortStream();
        _logger = logger;
        _parser = parser;
    }

    //public static string[] GetPortArray()
    //{
    //    return SerialPortStream.GetPortNames();
    //}

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
        //串口接收数据事件
        _serialPort.DataReceived += ReceiveDataMethod;
    }

    public void Open()
    {
        //打开串口
        try
        {
            _serialPort.Open();
            _logger.Info($"串口 {_serialPort.PortName} 连接成功");
        }
        catch (Exception ex)
        {
            _logger.Info($"打开串口 {_serialPort.PortName} 失败:");
            _logger.Info($"  错误信息: {ex.Message}");
            _logger.Info($"  堆栈跟踪: {ex.StackTrace}");
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
    /// 发送数据
    /// </summary>
    /// <param name="data">要发送的数据</param>
    public void SendDataMethod(string data)
    {
        //获取串口状态，true为已打开，false为未打开
        bool isOpen = _serialPort.IsOpen;

        if (!isOpen)
        {
            Open();
        }

        //直接发送字符串
        _serialPort.Write(data);
    }
    /// <summary>
    /// 串口接收到数据触发此方法进行数据读取
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ReceiveDataMethod(object sender, SerialDataReceivedEventArgs e)
    {
        byte[] readBuffer = new byte[ReadBufferSize];
        try
        {
            while (_serialPort.IsOpen && _serialPort.BytesToRead > 0)
            {
                var bytesRead = await _serialPort.ReadAsync(readBuffer, 0, ReadBufferSize);
                if (bytesRead > 0)
                {
                    _receiveBuffer.AddRange(readBuffer.Take(bytesRead));
                    await ProcessReceivedBuffer();
                }
                else
                {
                    await Task.Delay(10); // 避免忙等待
                }

                // 防止缓冲区无限增长
                if (_receiveBuffer.Count > MaxBufferSize)
                {
                    _logger.Info("接收缓冲区已满，清空。");
                    _receiveBuffer.Clear();
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Info("串口读取操作被取消。");
        }
        catch (Exception ex)
        {
            _logger.Info($"串口读取错误: {ex.Message}");
            _receiveBuffer.Clear(); // 发生错误时清理缓冲区
        }
        // ReceiveDataEventArg arg = new ReceiveDataEventArg();
        //
        // //读取串口缓冲区的字节数据
        // arg.Data = new byte[_serialPort.BytesToRead];
        // _serialPort.Read(arg.Data, 0, _serialPort.BytesToRead);
        //
        // Debug.WriteLine($"消息={string.Join(" ", arg.Data.Select(b => b.ToString("X2")))}");
        //
        // var result = BuildRequestInfo(arg.Data);
        // await result.Match(
        //     Right: async requestInfo =>
        //     {
        //         Console.WriteLine($"Request Info: {requestInfo.Data.Select(b => b.ToString("X2"))}");
        //         await this._parser.Route(requestInfo);
        //     },
        //     Left: async error =>
        //     {
        //         Console.WriteLine($"Error building request info: {error.Message}");
        //     });
        //
        // //触发自定义消息接收事件，把串口数据发送出去
        // if (ReceiveDataEvent != null && arg.Data.Length != 0)
        // {
        //     ReceiveDataEvent.Invoke(null, arg);
        // }
    }

    private async Task ProcessReceivedBuffer()
    {
        while (true)
        {
            int endIndex = IndexOf(_receiveBuffer, Svt.FullTail);
            if (endIndex != -1)
            {
                byte[] messageData = _receiveBuffer.Take(endIndex + Svt.FullTail.Length).ToArray();
                _receiveBuffer.RemoveRange(0, endIndex + Svt.FullTail.Length);

                if (messageData.Length > 0)
                {
                    ReceiveDataEventArg arg = new ReceiveDataEventArg { Data = messageData };
                    _logger.Info($"mcu消息={string.Join(" ", arg.Data.Select(b => b.ToString("X2")))}");

                    var result = BuildRequestInfo(arg.Data);
                    await result.Match(
                        Right: async requestInfo =>
                        {
                            _logger.Info($"Request Info: {requestInfo.DataFrame().Select(b => b.ToString("X2"))}");
                            await _parser.Route(requestInfo);
                        },
                        Left: async error =>
                        {
                            _logger.Info($"Error building request info: {error.Message}");
                        });

                    // if (ReceiveDataEvent != null)
                    // {
                    //     ReceiveDataEvent.Invoke(null, arg);
                    // }
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
                break;
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

    private static Either<Exception, SvtRequestInfo> BuildRequestInfo(byte[] data)
    {
        if (data.Length < Svt.MiniLength)
        {
            return Either<Exception, SvtRequestInfo>.Left(new ArgumentException($"Data length less than minimum required length: {Svt.MiniLength}"));
        }
        ReadOnlySpan<byte> span = data.AsSpan();
        // 先找包尾，有可能有多个包尾，取第一个
        var tailIndex = span.IndexOfFirst(0, span.Length, Svt.FullTail);
        if (tailIndex > 0)
        {
            // 从包尾往前找包头
            var headIndexes = span.IndexOfInclude(0, tailIndex - Svt.FullTail.Length, Svt.FullHead);
            // 找到取第一个
            if (headIndexes.Count > 0)
            {
                // 取距离包尾最近的包头
                var lastHeadIndex = headIndexes[^1];
                //var pos = byteBlock.Position;//记录初始游标位置，防止本次无法解析时，回退游标。
                // 去掉包头包尾
                var startIndex = lastHeadIndex + 1;
                var length = tailIndex + 1 - Svt.FullTail.Length - startIndex;
                var package = span.Slice(startIndex, length);
                // 去掉转义字符，才能校验包长度和CRC
                var cleanPackage = RemoveInsertedBytes(package);
                var myRequestInfo = new SvtRequestInfo();
                // 清理之后包长度，不含包头包尾最少是14
                if (cleanPackage.Length >= Svt.MiniLength - 4)
                {
                    int packetPos = 0;

                    // Read FrameNo (2 byte)
                    myRequestInfo.FrameNo = cleanPackage.Slice(packetPos, 2).ToArray();
                    packetPos += 2;

                    // Read PacketLength (2 byte)
                    myRequestInfo.PacketLength = TouchSocketBitConverter.BigEndian.ToUInt16(cleanPackage.Slice(packetPos, 2).ToArray(), 0);
                    packetPos += 2;

                    // Read AddressLength (1 byte)
                    myRequestInfo.AddressLength = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.MasterAddress = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.SlaveAddress = cleanPackage[packetPos];
                    packetPos += 1;

                    // Read CMDID (2 bytes)
                    myRequestInfo.CMDID = cleanPackage.Slice(packetPos, 2).ToArray();
                    packetPos += 2;

                    // Read FrameType (1 byte)
                    myRequestInfo.FrameType = cleanPackage[packetPos];
                    packetPos += 1;

                    myRequestInfo.DataLength = (ushort)((cleanPackage[packetPos] << 8) | cleanPackage[packetPos + 1]);
                    packetPos += 2;

                    // 数据长度校验，从包尾往前推，该校验可以去掉
                    if (packetPos + myRequestInfo.DataLength + 2 > cleanPackage.Length)
                    {
                        return Either<Exception, SvtRequestInfo>.Left(new ArgumentException("Data length Check Failed"));
                    }

                    myRequestInfo.Data = cleanPackage.Slice(packetPos, myRequestInfo.DataLength).ToArray();
                    packetPos += myRequestInfo.DataLength;

                    // Read CRC (1 byte)
                    myRequestInfo.CRC16 = cleanPackage.Slice(packetPos, 2).ToArray();
                    // packetPos += 2;

                    if (Crc16.ComputeCrc(cleanPackage, cleanPackage.Length - 2) ==
                            BitConverter.ToUInt16(myRequestInfo.CRC16.Reverse().ToArray(), 0))
                    {
                        return Either<Exception, SvtRequestInfo>.Right(myRequestInfo);
                    }
                    else
                    {
                        return Either<Exception, SvtRequestInfo>.Left(new ArgumentException("CRC Check Failed"));
                    }
                }
                else
                {
                    // 包有效长度不够，移动Position到该包尾的位置
                    return Either<Exception, SvtRequestInfo>.Left(new ArgumentException("Valid Length Not Enough")); ;
                }
            }
            else
            {
                // 找不到则移动Position到该包尾的位置
                return Either<Exception, SvtRequestInfo>.Left(new ArgumentException("No Head Found"));
            }
        }
        else
        {
            return Either<Exception, SvtRequestInfo>.Left(new ArgumentException("No Tail Found"));
        }
    }

    public async Task SendPacket(SvtRequestInfo packet)
    {
        await Task.Delay(10);
        var data = packet.BuildPackets();
        SendDataMethod(data);
        _logger.Info($"SendPacket: {string.Join(" ", data.Select(b => b.ToString("X2")))}");
    }

    private static ReadOnlySpan<byte> RemoveInsertedBytes(ReadOnlySpan<byte> data)
    {
        var cleanedData = new List<byte>();

        for (int i = 0; i < data.Length; i++)
        {
            // 添加当前字节
            cleanedData.Add(data[i]);

            // 检查是否需要跳过下一个字节
            if (i + 1 < data.Length)
            {
                if (data[i] == Svt.StartByte && data[i + 1] == Svt.InsertStartByte)
                {
                    // 跳过 0x82
                    i++;
                }
                else if (data[i] == Svt.EndByte && data[i + 1] == Svt.InsertEndByte)
                {
                    // 跳过 0x83
                    i++;
                }
            }
        }

        // 将列表转换为字节数组
        return cleanedData.ToArray().AsSpan();
    }
}

public class ReceiveDataEventArg : EventArgs
{
    /// <summary>
    /// 串口接收到的数据
    /// </summary>
    public byte[] Data { get; set; }
}