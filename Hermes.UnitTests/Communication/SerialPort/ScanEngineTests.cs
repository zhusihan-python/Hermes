using Hermes.Communication.SerialPort;
using System.Collections;
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
        Assert.True(_fixture.ScanEngine.IsOpen); // 验证串口是否连接成功
        Debug.WriteLine($"串口连接状态：{_fixture.ScanEngine.IsOpen}");
    }

    [Fact]
    public async Task SendThenReturn()
    {
        //var waitingClient = _fixture.ScanEngine.CreateWaitingClient(new WaitingOptions());
        //var data = new byte[] { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
        //var returnData = waitingClient.SendThenReturn(data);
        //if (returnData is null)
        //{
        //    Debug.WriteLine("returnData is null");
        //}
        //else
        //{
        //    var expectData = new byte[] { 0x32, 0x32, 0x31, 0x31, 0x31, 0x32, 0x36, 0x39, 0x0D, 0x0A };
        //    Debug.WriteLine($"Byte Array (Hex): [{string.Join(", ", returnData)}]");
        //    Assert.True(returnData.SequenceEqual(expectData));
        //}

        //var returnData2 = await waitingClient.SendThenReturnAsync(data);
        //Assert.True(returnData2.SequenceEqual(data));
    }

    [Fact]
    public async Task SendAsyncPacket()
    {
        var frameNumber = new byte[] { 0x00, 0x01 };
        var scanRequest = new ScanStartRequest(0x0001, frameNumber);
        await _fixture.ScanEngine.SendPacket(scanRequest);
    }
}

public class ScanEngineFixture
{
    public ScanEngine ScanEngine { get; private set; }

    public void InitializeAsync()
    {
        ScanEngine = new ScanEngine();
        ScanEngine.SetSerialPort("COM5", 9600); // 替换为你的串口名称和波特率
    }

    //public async Task DisposeAsync()
    //{
    //    await ScanEngine.ClientSafeCloseAsync();
    //}
}