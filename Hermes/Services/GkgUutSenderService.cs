using Hermes.Builders;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using Hermes.Types;

namespace Hermes.Services;

public class GkgUutSenderService : UutSenderService
{
    private const int Timeout = 2000;
    private const int MinDelayBetweenCycles = 3000;

    private SerialPort? _serialPort;
    private readonly SerialScanner _serialScanner;
    private string _serialNumberRead = "";
    private static int _triggerCount = 0;
    private readonly Stopwatch _stopwatch;
    private readonly Stopwatch _stopwatchBetweenCycles;

    public override string Path => SettingsRepository.Settings.GkgTunnelComPort;

    public GkgUutSenderService(
        Session session,
        ILogger logger,
        ISfcService sfcService,
        FileService fileService,
        ISettingsRepository settingsRepository,
        FolderWatcherService folderWatcherService,
        UnitUnderTestBuilder unitUnderTestBuilder,
        UnitUnderTestRepository unitUnderTestRepository,
        SfcResponseBuilder sfcResponseBuilder,
        SerialScanner serialScanner) : base(session, logger, sfcService, fileService, settingsRepository,
        folderWatcherService, unitUnderTestBuilder, unitUnderTestRepository, sfcResponseBuilder)
    {
        _serialScanner = serialScanner;
        this._stopwatch = new Stopwatch();
        this._stopwatchBetweenCycles = new Stopwatch();
        this._stopwatchBetweenCycles.Restart();
    }

    public override void Start()
    {
        if (IsRunning) return;
        _serialPort = new SerialPort(SettingsRepository.Settings.GkgTunnelComPort, 115200, Parity.None, 8,
            StopBits.One);
        _serialPort.DataReceived += OnDataReceived;
        _serialPort.Open();
        _serialScanner.Start();
        this.OnRunStatusChanged(true);
    }

    private async void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (this._stopwatchBetweenCycles.ElapsedMilliseconds <= MinDelayBetweenCycles && _triggerCount == 0) return;
        await ProcessTriggerSignal();
    }

    private async Task ProcessTriggerSignal()
    {
        try
        {
            this.Session.UutProcessorState = UutProcessorState.Processing;
            var instruction = _serialPort?.ReadExisting() ?? string.Empty;
            if (!instruction.Contains(SerialScanner.TriggerCommand)) return;

            Interlocked.Increment(ref _triggerCount);
            if (_triggerCount > 1) return;

            UnitUnderTest uut;
            var serialNumber = (await _serialScanner.Scan()).Replace("ERROR", "");
            if (string.IsNullOrEmpty(serialNumber))
            {
                uut = _unitUnderTestBuilder
                    .Clone()
                    .ScanError(true)
                    .Build();
            }
            else
            {
                uut = _unitUnderTestBuilder
                    .Clone()
                    .FileNameWithoutExtension($"{serialNumber}_{DateTime.Now:yyMMddHHmmss}")
                    .SerialNumber(serialNumber)
                    .Build();
                this.OnUnitUnderTestCreated(uut);
                await this.SendUnitUnderTest(uut);
            }

            this.OnSfcResponse(uut);

            if (uut.SfcResponse is { IsFail: false })
            {
                await this.WaitForSecondTrigger();
                _serialPort?.Write($"{serialNumber}{SerialScanner.LineTerminator}");
            }
        }
        catch (Exception exception)
        {
            var uut = _unitUnderTestBuilder
                .ResponseFailMessage(exception.Message)
                .Build();
            Logger.Error(exception.Message);
            this.OnSfcResponse(uut);
        }

        Interlocked.Exchange(ref _triggerCount, 0);
        this.Session.UutProcessorState = UutProcessorState.Idle;
        this._stopwatchBetweenCycles.Restart();
    }

    private async Task WaitForSecondTrigger()
    {
        this._stopwatch.Restart();
        while (_triggerCount < 2 && this._stopwatch.ElapsedMilliseconds <= Timeout)
        {
            await Task.Delay(this.SettingsRepository.Settings.WaitDelayMilliseconds);
        }

        this._stopwatch.Stop();
    }

    public override void Stop()
    {
        if (!IsRunning) return;
        this._serialPort?.Close();
        _serialPort?.Dispose();
        this.CancellationTokenSource?.Cancel();
        _serialScanner.Stop();
        this.OnRunStatusChanged(false);
    }
}