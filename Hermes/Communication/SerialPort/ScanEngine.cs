using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace Hermes.Communication.SerialPort;

public class ScanEngine
{
    private SerialPortClient _client;
    public bool ClientOnline => _client.Online;

    public ScanEngine()
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
            Debug.WriteLine($"RequestInfo {e.RequestInfo}");
            await Task.Delay(100);
            //await ProcessReceivedDataAsync(myRequest);
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
            .SetSerialDataHandlingAdapter(() => new ScanDataHandlingAdapter()) // 数据适配器
            .ConfigurePlugins(a =>
            {
                a.Add<ScannerConnectingPlugin>();
                a.Add<ScannerConnectedPlugin>();
                a.Add<ScannerReceivedPlugin>();
                a.Add<ScannerClosedPlugin>();
            }));

        // 连接串口
        await _client.ConnectAsync();
        Debug.WriteLine($"串口 {portName} 连接成功");
    }

    public async Task ClientSafeCloseAsync()
    {
        await _client.SafeCloseAsync();
    }

    public IWaitingClient<ISerialPortClient, IReceiverResult> CreateWaitingClient(WaitingOptions options)
    {
        var client = _client!.CreateWaitingClient(options);
        return client;
    }

    // 异步处理数据的示例方法
    private async Task ProcessReceivedDataAsync(ScanRequestInfo request)
    {
        // 模拟异步操作，如存储数据或发送响应
        //await Task.Delay(100); // Simulate I/O or processing delay
        //await this._parser.Route(request);
        Debug.WriteLine($"处理完成：来自{request.dataFrame}的消息已处理。");
    }

    internal class ScannerConnectingPlugin : PluginBase, ISerialConnectingPlugin
    {
        public async Task OnSerialConnecting(ISerialPortSession client, ConnectingEventArgs e)
        {
            Debug.WriteLine("准备连接串口");
            await e.InvokeNext();
        }
    }

    internal class ScannerConnectedPlugin : PluginBase, ISerialConnectedPlugin
    {
        public async Task OnSerialConnected(ISerialPortSession client, ConnectedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    internal class ScannerReceivedPlugin : PluginBase, ISerialReceivedPlugin
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

    internal class ScannerClosedPlugin : PluginBase, ISerialClosedPlugin
    {
        public async Task OnSerialClosed(ISerialPortSession client, ClosedEventArgs e)
        {
            await e.InvokeNext();
        }
    }

    public async Task SendPacketAsync(ScanRequestInfo packet)
    {
        await Task.Delay(100);
        Debug.WriteLine($"Send ScanRequestInfo: {string.Join(" ", packet.dataFrame.Select(b => b.ToString("X2")))}");
        await this._client.SendAsync(packet);
    }
}
