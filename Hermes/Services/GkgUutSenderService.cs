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
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Hermes.Services;

public partial class GkgUutSenderService : UutSenderService
{
    private const int TimeoutBetweenTriggers = 2000;
    private const int MinDelayBetweenCycles = 8000;

    private SerialPort _serialPort;
    private readonly SerialScanner _serialScanner;
    private readonly Session _session;
    private readonly Stopwatch _stopwatch;
    private readonly Stopwatch _stopwatchBetweenCycles;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private IObservable<string> _serialNumberScanned = null!;
    private IDisposable _validSerialNumberObserver = null!;
    private IDisposable _notValidSerialNumberObserver = null!;
    private IObservable<string> _triggerReceived;
    private IDisposable _sendDummyUnitUnderTestObserver;
    private readonly ILogger _logger;
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
        this._logger = logger;
        this._serialScanner = serialScanner;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._serialPort = new SerialPort();
        this._stopwatch = new Stopwatch();
        this._stopwatchBetweenCycles = new Stopwatch();
        this._stopwatchBetweenCycles.Restart();
        this.SetupReactiveExtensions();
    }

    private void SetupReactiveExtensions()
    {
        this._triggerReceived = Observable
            .FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                x => _serialPort.DataReceived += x,
                x => _serialPort.DataReceived -= x)
            .Delay(TimeSpan.FromMilliseconds(20))
            .Select(x => this.SerialReadExisting())
            .Where(x => x.Contains(SerialScanner.TriggerCommand));

        this._serialNumberScanned = this._triggerReceived
            .SelectMany(async x =>
            {
                _logger.Debug("Scanning");
                this._session.UutProcessorState = UutProcessorState.Scanning;
                return await this._serialScanner.Scan();
            })
            .Select(x => x.Replace("ERROR", "").Trim())
            .Select(x =>
            {
                _logger.Debug("Scanned");
                if (this.IsWaitingForDummy)
                {
                    this.IsWaitingForDummy = false;
                    return string.IsNullOrEmpty(x) ? SerialScanner.DummySerialNumber : x;
                }

                return x;
            });
    }

    private void SetupReactiveObservers()
    {
        var serialNumberReceivedSubject = new Subject<string>();
        this._serialNumberScanned
            .SubscribeOn(SynchronizationContext.Current!)
            .Subscribe(serialNumber =>
            {
                _logger.Debug("TriggerReceived");
                this._session.UutProcessorState = UutProcessorState.Processing;
                serialNumberReceivedSubject.OnNext(serialNumber);
                this.IsWaitingForDummy = false;
                this._session.UutProcessorState = UutProcessorState.Idle;
                _logger.Debug("TriggerEnd");
            });

        this._sendDummyUnitUnderTestObserver = serialNumberReceivedSubject
            .Where(serialNumber => serialNumber == SerialScanner.DummySerialNumber)
            .Subscribe(serialNumber =>
            {
                _logger.Debug($"Dummy enabled: {serialNumber}");
                this.SendDummyUnitUnderTest();
            });

        this._validSerialNumberObserver = serialNumberReceivedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(serialNumber => !string.IsNullOrEmpty(serialNumber))
            .SelectMany(async serialNumber =>
            {
                _logger.Debug($"SendSerialNumber: {serialNumber}");
                await this.SendSerialNumber(serialNumber);
                return serialNumber;
            })
            .Subscribe(
                _ => _logger.Debug($"NextScanning"),
                _ => this.Stop());

        this._notValidSerialNumberObserver = serialNumberReceivedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(string.IsNullOrEmpty)
            .Subscribe(
                _ => this.SendScanErrorUnitUnderTest(),
                _ => this.Stop());
    }

    private async Task SendSerialNumber(string serialNumber)
    {
        this._session.UutProcessorState = UutProcessorState.Processing;
        var unitUnderTest = this.BuildUnitUnderTest(serialNumber);
        this.OnUnitUnderTestCreated(unitUnderTest);
        await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
        await this.SendUnitUnderTest(unitUnderTest);
        await this._unitUnderTestRepository.SaveChangesAsync();
        if (!unitUnderTest.IsSfcFail ||
            unitUnderTest.SfcResponseContains(SettingsRepository.Settings.AdditionalOkSfcResponse))
        {
            _serialPort?.Write($"{serialNumber}{SerialScanner.LineTerminator}");
            _logger.Debug($"Responded to trigger");
        }

        this.OnSfcResponse(unitUnderTest);
        this._session.UutProcessorState = UutProcessorState.Idle;
    }

    private void SendScanErrorUnitUnderTest()
    {
        this.OnSfcResponse(this._unitUnderTestBuilder
            .Clone()
            .ScanError(true)
            .Build());
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

    public override void Start()
    {
        if (IsRunning) return;
        this.IsRunning = true;
        try
        {
            this.SetupReactiveObservers();
            this._serialPort.PortName = SettingsRepository.Settings.GkgTunnelComPort;
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

    private async void OnDataReceived(SerialDataReceivedEventArgs e)
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
        this._session.UutProcessorState = UutProcessorState.Processing;
        var unitUnderTest = this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{UnitUnderTestBuilder.DummySerialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(UnitUnderTestBuilder.DummySerialNumber)
            .SetOkResponse()
            .Build();
        this.OnSfcResponse(unitUnderTest);
        _serialPort?.Write($"{UnitUnderTestBuilder.DummySerialNumber}{SerialScanner.LineTerminator}");
        this._session.UutProcessorState = UutProcessorState.Idle;
    }

    private async Task ProcessTrigger()
    {
        UnitUnderTest unitUnderTest = BuildScanErrorUnitUnderTest();
        try
        {
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
        this._validSerialNumberObserver?.Dispose();
        this._notValidSerialNumberObserver?.Dispose();
        this._sendDummyUnitUnderTestObserver?.Dispose();
        this._serialPort?.Close();
        this._serialPort?.Dispose();
        this._serialScanner.Stop();
        this.OnRunStatusChanged(false);
    }
}