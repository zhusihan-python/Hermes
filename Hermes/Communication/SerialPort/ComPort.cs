using System.Threading.Tasks;
using System;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;
using System.Diagnostics;
using System.Linq;

namespace Hermes.Communication.SerialPort;

public class ComPort
{
    private static readonly Lazy<ComPort> _instance = new(() => new ComPort());
    public static ComPort Instance => _instance.Value;
    private SerialPortClient _client;

    public ComPort()
    {
        _client = new SerialPortClient();
    }

    public async Task InitializeAsync(string portName, int baudRate)
    {
        // 设置事件回调
        _client.Connecting = (client, e) => { return EasyTask.CompletedTask; }; // 即将连接到端口
        _client.Connected = (client, e) => { return EasyTask.CompletedTask; }; // 成功连接到端口
        _client.Closing = (client, e) => { return EasyTask.CompletedTask; };   // 即将从端口断开连接
        _client.Closed = (client, e) => { return EasyTask.CompletedTask; };    // 从端口断开连接

        // 接收数据事件
        _client.Received = (c, e) =>
        {
            if (e.RequestInfo is SvtRequestInfo myRequest)
            {
                Debug.WriteLine($"已从{myRequest.MasterAddress}接收到：CMDID={string.Join(" ", myRequest.CMDID.Select(b => b.ToString("X2")))}," +
                    $"FrameType={myRequest.FrameType},消息={string.Join(" ", myRequest.Data.Select(b => b.ToString("X2")))}");
            }
            return Task.CompletedTask;
        };

        // 配置串口参数
        await _client.SetupAsync(new TouchSocketConfig()
            .SetSerialPortOption(new SerialPortOption()
            {
                PortName = portName,       // 串口号
                BaudRate = baudRate,       // 波特率
                DataBits = 8,              // 数据位
                Parity = System.IO.Ports.Parity.None, // 校验位
                StopBits = System.IO.Ports.StopBits.One // 停止位
            })
            .SetSerialDataHandlingAdapter(() => new SvtDataHandlingAdapter()) // 数据适配器
            .ConfigurePlugins(a =>
            {
                a.Add<MyConnectingPlugin>();
                a.Add<MyConnectedPlugin>();
                a.Add<MyReceivedPlugin>();
                a.Add<MyClosedPlugin>();
            }));

        // 连接串口
        await _client.ConnectAsync();
        Debug.WriteLine("串口连接成功");
        //_client.Close();
    }

    internal class MyConnectingPlugin : PluginBase, ISerialConnectingPlugin
    {
        public async Task OnSerialConnecting(ISerialPortSession client, ConnectingEventArgs e)
        {
            Debug.WriteLine("准备连接串口");
            await e.InvokeNext();
        }
    }

    internal class MyConnectedPlugin : PluginBase, ISerialConnectedPlugin
    {
        public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    internal class MyReceivedPlugin : PluginBase, ISerialReceivedPlugin
    {
        public async Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e)
        {
            //这里处理数据接收
            //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
            var byteBlock = e.ByteBlock;
            var requestInfo = e.RequestInfo;

            //e.Handled = true;//表示该数据已经被本插件处理，无需再投递到其他插件。

            await e.InvokeNext();
        }
    }

    internal class MyClosedPlugin : PluginBase, ISerialClosedPlugin
    {
        public async Task OnSerialClosed(ISerialPortSession client, ClosedEventArgs e)
        {
            await e.InvokeNext();
        }
    }
}
