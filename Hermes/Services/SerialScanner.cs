using Hermes.Types;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using Hermes.Models;
using Reactive.Bindings;

namespace Hermes.Services;

public class SerialScanner
{
    public const string DummySerialNumber = "Dummy";
    public const string TriggerCommand = "LOF";
    public const string LineTerminator = "\r";
    private const int Timeout = 5000;

    public ReactiveProperty<StateType> State { get; } = new(StateType.Stopped);
    public ReactiveProperty<string> ScannedText { get; } = new("");

    private SerialPort? _serialPort;
    private bool _isWaitingForData;
    private readonly Settings _settings;
    private readonly Stopwatch _stopwatch;

    public SerialScanner(Settings settings)
    {
        this._settings = settings;
        this._stopwatch = new Stopwatch();
    }

    public string PortName => _settings.ScannerComPort;

    public void Open()
    {
        try
        {
            if (_serialPort is { IsOpen: true }) return;

            this._serialPort = new SerialPort(_settings.ScannerComPort, 115200, Parity.None,
                8,
                StopBits.One);
            this._serialPort.DataReceived += Proxy;
            this._serialPort.Open();
            this.State.Value = StateType.Idle;
        }
        catch (Exception e)
        {
            this.Close();
            throw;
        }
    }

    public void Close()
    {
        this._serialPort?.Close();
        this._serialPort?.Dispose();
        this.State.Value = StateType.Stopped;
    }

    public async Task<string> Scan()
    {
        if (_serialPort is not { IsOpen: true }) return "";

        this.State.Value = StateType.Processing;

        _serialPort.DiscardInBuffer();
        await WriteAsync(TriggerCommand + LineTerminator);
        var scannedText = "";
        if (await WaitForData(Timeout))
        {
            scannedText = await ReadAllAsync();
        }
        else
        {
            _serialPort.DiscardInBuffer();
        }

        ScannedText.Value = scannedText;
        this.State.Value = StateType.Idle;
        return scannedText;
    }

    private async Task WriteAsync(string command)
    {
        try
        {
            if (_serialPort is not { IsOpen: true }) return;
            using var cts = new CancellationTokenSource(500);
            await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(command + LineTerminator), cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<string> ReadAllAsync()
    {
        if (_serialPort is not { IsOpen: true }) return string.Empty;

        try
        {
            var expectedBytes = _serialPort.BytesToRead;
            var buffer = new byte[expectedBytes];
            var readBytes = 0;
            using var memoryStream = new MemoryStream();
            using var cts = new CancellationTokenSource(500);

            do
            {
                readBytes += await _serialPort.BaseStream.ReadAsync(
                    buffer.AsMemory(readBytes, expectedBytes), cts.Token);
                memoryStream.Write(buffer, 0, readBytes);
            } while (readBytes < expectedBytes);

            return Encoding.ASCII.GetString(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            return string.Empty;
        }
    }

    private void Proxy(object _, SerialDataReceivedEventArgs __)
    {
        _isWaitingForData = false;
    }

    private async Task<bool> WaitForData(int timeout)
    {
        _isWaitingForData = true;
        this._stopwatch.Restart();
        while (_isWaitingForData && this._stopwatch.ElapsedMilliseconds <= timeout)
        {
            await Task.Delay(this._settings.WaitDelayMilliseconds);
        }

        this._stopwatch.Stop();
        return this._stopwatch.ElapsedMilliseconds < timeout;
    }
}