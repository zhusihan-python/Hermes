using Hermes.Builders;
using Hermes.Common.Aspects;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public partial class GkgUutSenderService : UutSenderService
{
    private const int TimeoutBetweenTriggers = 2000;
    private const int MinDelayBetweenCycles = 3000;

    private SerialPort? _serialPort;
    private readonly SerialScanner _serialScanner;
    private readonly Session _session;
    private readonly Stopwatch _stopwatch;
    private readonly Stopwatch _stopwatchBetweenCycles;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private static int _triggerCount;

    public override string Path => SettingsRepository.Settings.GkgTunnelComPort;

    public GkgUutSenderService(
        ILogger logger,
        ISettingsRepository settingsRepository,
        ISfcService sfcService,
        SerialScanner serialScanner,
        Session session,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder)
        : base(logger, sfcService, settingsRepository, sfcResponseBuilder)
    {
        this._session = session;
        this._serialScanner = serialScanner;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._stopwatch = new Stopwatch();
        this._stopwatchBetweenCycles = new Stopwatch();
        this._stopwatchBetweenCycles.Restart();
    }

    public override void Start()
    {
        try
        {
            if (IsRunning) return;
            this.IsRunning = true;
            this._serialPort = new SerialPort(
                SettingsRepository.Settings.GkgTunnelComPort,
                115200,
                Parity.None,
                8,
                StopBits.One);
            this._serialPort.DataReceived += OnDataReceived;
            this._serialPort.Open();
            this._serialScanner.Start();
            this.OnRunStatusChanged(true);
        }
        catch (Exception)
        {
            this.Stop();
            throw;
        }
    }

    private async void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (!CanStartNewCycle) return;
        await this.ProcessTriggerSignal();
    }

    private bool CanStartNewCycle => this._stopwatchBetweenCycles.ElapsedMilliseconds > MinDelayBetweenCycles &&
                                     this._session.UutProcessorState == UutProcessorState.Idle;

    private async Task ProcessTriggerSignal()
    {
        try
        {
            this._session.UutProcessorState = UutProcessorState.Processing;
            var command = _serialPort?.ReadExisting() ?? string.Empty;
            if (!command.Contains(SerialScanner.TriggerCommand)) return;

            Interlocked.Increment(ref _triggerCount);
            if (_triggerCount > 1) return;

            UnitUnderTest unitUnderTest = BuildScanErrorUnitUnderTest();
            var serialNumber = (await this._serialScanner.Scan())
                .Replace("ERROR", "")
                .Trim();
            if (!string.IsNullOrEmpty(serialNumber))
            {
                unitUnderTest = this.BuildUnitUnderTest(serialNumber);
                this.OnUnitUnderTestCreated(unitUnderTest);
                await this.SendUnitUnderTest(unitUnderTest);
            }

            this.OnSfcResponse(unitUnderTest);

            if (unitUnderTest.SfcResponse is { IsFail: false })
            {
                await this.WaitForSecondTrigger();
                _serialPort?.Write($"{serialNumber}{SerialScanner.LineTerminator}");
            }
        }
        catch (Exception exception)
        {
            // TODO: Show stop screen when a failure occurs or retry
            var uut = this._unitUnderTestBuilder
                .Clone()
                .ResponseFailMessage(exception.Message)
                .Build();
            Logger.Error(exception.Message);
            this.OnSfcResponse(uut);
        }

        Interlocked.Exchange(ref _triggerCount, 0);
        this._session.UutProcessorState = UutProcessorState.Idle;
        this._stopwatchBetweenCycles.Restart();
    }

    private UnitUnderTest BuildUnitUnderTest(string serialNumber)
    {
        return this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{serialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(serialNumber)
            .Build();
    }

    private UnitUnderTest BuildScanErrorUnitUnderTest()
    {
        return this._unitUnderTestBuilder
            .Clone()
            .ScanError(true)
            .Build();
    }

    private async Task WaitForSecondTrigger()
    {
        this._stopwatch.Restart();
        while (_triggerCount < 2 && this._stopwatch.ElapsedMilliseconds <= TimeoutBetweenTriggers)
        {
            await Task.Delay(this.SettingsRepository.Settings.WaitDelayMilliseconds);
        }

        this._stopwatch.Stop();
    }

    public override void Stop()
    {
        if (!IsRunning) return;
        this.IsRunning = false;
        this._serialPort?.Close();
        this._serialPort?.Dispose();
        this._serialScanner.Stop();
        this.OnRunStatusChanged(false);
    }
}