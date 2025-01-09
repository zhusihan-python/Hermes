using System.Text;
using System.Threading.Tasks;
using System;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace Hermes.Communication.SerialPort;

public class ComPort
{
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
        _client.Received = async (c, e) =>
        {
            await Console.Out.WriteLineAsync(e.ByteBlock.Span.ToString(Encoding.UTF8));
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
                a.Add<MyConnectedPlugin>();
                a.Add<MyReceivedPlugin>();
                a.Add<MyClosedPlugin>();
            }));

        // 连接串口
        await _client.ConnectAsync();
        Console.WriteLine("串口连接成功");
        _client.Close();
    }

    internal class MyConnectedPlugin : PluginBase, ISerialConnectedPlugin
    {
        public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    public class MyReceivedPlugin : PluginBase, ISerialReceivedPlugin
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

    public class MyClosedPlugin : PluginBase, ISerialClosedPlugin
    {
        public async Task OnSerialClosed(ISerialPortSession client, ClosedEventArgs e)
        {
            await e.InvokeNext();
        }
    }
}
