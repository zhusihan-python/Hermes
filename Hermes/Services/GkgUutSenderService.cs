using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.IO.Ports;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public class GkgUutSenderService : UutSenderService
{
    private IObservable<string> _serialNumberScanned = null!;
    private IObservable<string> _triggerReceived;
    private readonly CompositeDisposable _disposables = [];
    private readonly ILogger _logger;
    private readonly SerialPort _serialPort;
    private readonly SerialScanner _serialScanner;
    private readonly Session _session;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

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
            .Where(x => x.Contains(SerialScanner.TriggerCommand))
            .TakeFirstAndThrottle(TimeSpan.FromSeconds(5));

        this._serialNumberScanned = this._triggerReceived
            .SelectMany(async x =>
            {
                _logger.Debug("Scanning");
                this.State.Value = UutProcessorState.Scanning;
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
        var serialNumberDisposable = this._serialNumberScanned
            .SubscribeOn(SynchronizationContext.Current!)
            .Subscribe(serialNumber =>
            {
                // TODO: remove this
                _logger.Debug("TriggerReceived");
                this._session.UutProcessorState = UutProcessorState.Processing;
                serialNumberReceivedSubject.OnNext(serialNumber);
                this.IsWaitingForDummy = false;
                this._session.UutProcessorState = UutProcessorState.Idle;
                _logger.Debug("TriggerEnd");
            });

        var sendDummyUnitUnderTestDisposable = serialNumberReceivedSubject
            .Where(serialNumber => serialNumber == SerialScanner.DummySerialNumber)
            .Subscribe(serialNumber =>
            {
                _logger.Debug($"Dummy enabled: {serialNumber}");
                this.SendDummyUnitUnderTest();
            });

        var validSerialNumberDisposable = serialNumberReceivedSubject
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

        var notValidSerialNumberDisposable = serialNumberReceivedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(string.IsNullOrEmpty)
            .Subscribe(
                _ => this.SendScanErrorUnitUnderTest(),
                _ => this.Stop());

        this._disposables.Add(serialNumberDisposable);
        this._disposables.Add(sendDummyUnitUnderTestDisposable);
        this._disposables.Add(validSerialNumberDisposable);
        this._disposables.Add(notValidSerialNumberDisposable);
    }

    private async Task SendSerialNumber(string serialNumber)
    {
        this.State.Value = UutProcessorState.Processing;
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
        this.State.Value = UutProcessorState.Idle;
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

    private void SendDummyUnitUnderTest()
    {
        this.State.Value = UutProcessorState.Processing;
        var unitUnderTest = this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{UnitUnderTestBuilder.DummySerialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(UnitUnderTestBuilder.DummySerialNumber)
            .SetOkResponse()
            .Build();
        this.OnSfcResponse(unitUnderTest);
        _serialPort?.Write($"{UnitUnderTestBuilder.DummySerialNumber}{SerialScanner.LineTerminator}");
        this.State.Value = UutProcessorState.Idle;
    }

    private UnitUnderTest BuildUnitUnderTest(string serialNumber)
    {
        return this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{serialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(serialNumber)
            .Build();
    }

    public override void Stop()
    {
        if (!IsRunning) return;
        this.IsRunning = false;
        this._disposables.Dispose();
        this._serialPort?.Close();
        this._serialScanner.Stop();
        this.OnRunStatusChanged(false);
    }
}