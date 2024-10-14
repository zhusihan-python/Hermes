using Hermes.Common.Aspects;
using Hermes.Repositories;
using Hermes.Types;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Hermes.Services;

public class SerialScanner
{
    public const string TriggerCommand = "LOF";
    public const string LineTerminator = "\r";
    private const int Timeout = 5000;

    public event Action<StateType>? StateChanged;
    public event Action<string>? Scanned;

    private SerialPort? _serialPort;
    private bool _isWaitingForData;
    private readonly ISettingsRepository _settingsRepository;
    private readonly Stopwatch _stopwatch;

    public SerialScanner(ISettingsRepository settingsRepository)
    {
        this._settingsRepository = settingsRepository;
        this._stopwatch = new Stopwatch();
    }

    public string PortName => _settingsRepository.Settings.ScannerComPort;

    [LogException]
    public void Start()
    {
        try
        {
            if (_serialPort is { IsOpen: true }) return;
            
            this._serialPort = new SerialPort(_settingsRepository.Settings.ScannerComPort, 115200, Parity.None,
                8,
                StopBits.One);
            this._serialPort.DataReceived += Proxy;
            this._serialPort.Open();
            this.StateChanged?.Invoke(StateType.Idle);
        }
        catch (Exception e)
        {
            this.Stop();
            throw;
        }
    }

    [LogException]
    public void Stop()
    {
        this._serialPort?.Close();
        this._serialPort?.Dispose();
        this.StateChanged?.Invoke(StateType.Stopped);
    }

    [LogException]
    public async Task<string> Scan()
    {
        if (_serialPort is not { IsOpen: true }) return "";

        this.StateChanged?.Invoke(StateType.Processing);

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

        this.Scanned?.Invoke(scannedText);
        this.StateChanged?.Invoke(StateType.Idle);
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
            await Task.Delay(this._settingsRepository.Settings.WaitDelayMilliseconds);
        }

        this._stopwatch.Stop();
        return this._stopwatch.ElapsedMilliseconds < timeout;
    }
}