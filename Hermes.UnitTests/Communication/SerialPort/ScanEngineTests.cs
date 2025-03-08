using Hermes.Communication.SerialPort;
using System.Diagnostics;
using TouchSocket.Sockets;

namespace HermesTests.Communication.SerialPort;

public class ScanEngineTests : IClassFixture<ScanEngineFixture>
{
    private readonly ScanEngineFixture _fixture;
    public ScanEngineTests(ScanEngineFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ScanEngine_InitializeAsync_ConnectsToSerialPort()
    {
        // Assert
        Assert.True(_fixture.ScanEngine.ClientOnline); // 验证串口是否连接成功
        Debug.WriteLine($"串口连接状态：{_fixture.ScanEngine.ClientOnline}");
    }

    [Fact]
    public async Task SendThenReturn()
    {
        var waitingClient = _fixture.ScanEngine.CreateWaitingClient(new WaitingOptions());
        var data = new byte[] { 0, 1, 2, 3, 4 };
        var returnData = waitingClient.SendThenReturn(data);
        Assert.True(returnData.SequenceEqual(data));

        var returnData2 = await waitingClient.SendThenReturnAsync(data);
        Assert.True(returnData2.SequenceEqual(data));
    }
}

public class ScanEngineFixture : IAsyncLifetime
{
    public ScanEngine ScanEngine { get; private set; }

    public async Task InitializeAsync()
    {
        ScanEngine = new ScanEngine();
        await ScanEngine.InitializeAsync("COM7", 9600); // 替换为你的串口名称和波特率
    }

    public async Task DisposeAsync()
    {
        await ScanEngine.ClientSafeCloseAsync();
    }
}