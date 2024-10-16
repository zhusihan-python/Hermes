using Hermes.Builders;
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
    private const int MinDelayBetweenCycles = 8000;

    private SerialPort? _serialPort;
    private readonly SerialScanner _serialScanner;
    private readonly Session _session;
    private readonly Stopwatch _stopwatch;
    private readonly Stopwatch _stopwatchBetweenCycles;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private static int _triggerCount;

    public override string Path => SettingsRepository.Settings.GkgTunnelComPort;

    public GkgUutSenderService(
        ILogger logger,
        ISettingsRepository settingsRepository,
        ISfcService sfcService,
        SerialScanner serialScanner,
        Session session,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder,
        UnitUnderTestRepository unitUnderTestRepository)
        : base(logger, sfcService, settingsRepository, sfcResponseBuilder)
    {
        this._session = session;
        this._serialScanner = serialScanner;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._unitUnderTestRepository = unitUnderTestRepository;
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

        this._session.UutProcessorState = UutProcessorState.Processing;
        if (IsWaitingForDummy)
        {
            this.SendDummyUnitUnderTest();
        }
        else
        {
            await this.ProcessTriggerSignal();
        }

        this._session.UutProcessorState = UutProcessorState.Idle;
    }

    private bool CanStartNewCycle => this._stopwatchBetweenCycles.ElapsedMilliseconds > MinDelayBetweenCycles &&
                                     this._session.UutProcessorState == UutProcessorState.Idle;

    private void SendDummyUnitUnderTest()
    {
        var unitUnderTest = this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{UnitUnderTestBuilder.DummySerialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(UnitUnderTestBuilder.DummySerialNumber)
            .SetOkResponse()
            .Build();
        this.OnSfcResponse(unitUnderTest);
        _serialPort?.Write($"{UnitUnderTestBuilder.DummySerialNumber}{SerialScanner.LineTerminator}");
    }

    private async Task ProcessTriggerSignal()
    {
        UnitUnderTest unitUnderTest = BuildScanErrorUnitUnderTest();
        try
        {
            var command = _serialPort?.ReadExisting() ?? string.Empty;
            if (!command.Contains(SerialScanner.TriggerCommand)) return;

            Interlocked.Increment(ref _triggerCount);
            if (_triggerCount > 1) return;

            var serialNumber = (await this._serialScanner.Scan())
                .Replace("ERROR", "")
                .Trim();
            if (!string.IsNullOrEmpty(serialNumber))
            {
                unitUnderTest = this.BuildUnitUnderTest(serialNumber);
                this.OnUnitUnderTestCreated(unitUnderTest);
                await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
                await this.SendUnitUnderTest(unitUnderTest);
                await this._unitUnderTestRepository.SaveChangesAsync();
            }

            this.OnSfcResponse(unitUnderTest);

            if (!unitUnderTest.IsSfcFail ||
                unitUnderTest.SfcResponseContains(SettingsRepository.Settings.AdditionalOkSfcResponse))
            {
                await this.WaitForSecondTrigger();
                _serialPort?.Write($"{serialNumber}{SerialScanner.LineTerminator}");
            }
        }
        catch (Exception e) when (e is not TimeoutException)
        {
            unitUnderTest = this._unitUnderTestBuilder
                .Clone()
                .ResponseFailMessage(e.Message)
                .Build();
            Logger.Error(e.Message);
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }

        this.OnSfcResponse(unitUnderTest);
        Interlocked.Exchange(ref _triggerCount, 0);
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