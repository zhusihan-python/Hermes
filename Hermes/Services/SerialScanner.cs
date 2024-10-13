using Hermes.Common.Aspects;
using Hermes.Repositories;
using Hermes.Types;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System;

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
            this._serialPort = new SerialPort(_settingsRepository.Settings.ScannerComPort, 115200, Parity.None, 8,
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

        _serialPort.WriteLine(TriggerCommand + LineTerminator);
        var scannedText = "";
        if (await WaitForData(Timeout))
        {
            scannedText = _serialPort.ReadExisting();
        }
        else
        {
            _serialPort.DiscardInBuffer();
        }

        this.Scanned?.Invoke(scannedText);
        this.StateChanged?.Invoke(StateType.Idle);
        return scannedText;
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