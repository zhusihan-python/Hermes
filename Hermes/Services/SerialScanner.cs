using Hermes.Common;
using Hermes.Repositories;
using Hermes.Types;
using System.IO.Ports;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;

namespace Hermes.Services;

public class SerialScanner
{
    public const string TriggerCommand = "LOF";
    public const string LineTerminator = "\r";
    private const int Timeout = 5000;

    private readonly object _serialIncoming = new();
    private bool _isWaitingForData;
    private readonly Stopwatch _stopwatch;
    private SerialPort? _serialPort;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger _logger;
    public event Action<StateType>? StateChanged;
    public event Action<string>? Scanned;

    public SerialScanner(ISettingsRepository settingsRepository,
        ILogger logger)
    {
        this._settingsRepository = settingsRepository;
        this._logger = logger;
        this._stopwatch = new Stopwatch();
    }

    public string PortName => _settingsRepository.Settings.ScannerComPort;

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
            _logger.Error(e.Message);
            this.Stop();
            throw;
        }
    }

    public void Stop()
    {
        try
        {
            this._serialPort?.Close();
            this._serialPort?.Dispose();
            this.StateChanged?.Invoke(StateType.Stopped);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
        }
    }

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