using System.IO.Ports;
using System.Reactive.Linq;
using System;

namespace Hermes.Common.Reactive;

public class SerialPortRx : IDisposable
{
    public IObservable<string> DataReceived { get; private set; }
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public StopBits StopBits { get; set; } = StopBits.One;
    public Parity Parity { get; set; } = Parity.None;
    public Handshake Handshake { get; set; } = Handshake.None;
    public int ReadTimeout { get; set; } = 5000;

    private readonly SerialPort _serialPort;

    public SerialPortRx(SerialPort serialPort)
    {
        this._serialPort = serialPort;

        DataReceived = Observable
            .FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                x => serialPort.DataReceived += x,
                x => serialPort.DataReceived -= x)
            .Select(x => x.EventArgs)
            .Delay(TimeSpan.FromMilliseconds(20))
            .Select(x => this.SerialReadExisting());
    }

    public SerialPortRx()
        : this(new SerialPort())
    {
    }

    private string SerialReadExisting()
    {
        try
        {
            return _serialPort.ReadExisting();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public void Open()
    {
        _serialPort.PortName = PortName;
        _serialPort.BaudRate = BaudRate;
        _serialPort.DataBits = DataBits;
        _serialPort.StopBits = StopBits;
        _serialPort.Parity = Parity;
        _serialPort.Handshake = Handshake;
        _serialPort.ReadTimeout = ReadTimeout;
        _serialPort.Open();
    }

    public void Close()
    {
        _serialPort.Close();
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }

    public void Write(string text)
    {
        _serialPort.Write(text);
    }
}