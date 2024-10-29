using Hermes.Builders;
using Hermes.Common.Reactive;
using Hermes.Common;
using Hermes.Models;
using Hermes.Types;
using R3;
using System.Threading.Tasks;
using System;

namespace Hermes.Services.UutSenderService;

public class GkgUutSenderService : UutSenderService
{
    private readonly ILogger _logger;
    private readonly SerialPortRx _serialPortRx;
    private readonly SerialScanner _serialScanner;
    private readonly Settings _settings;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private Subject<string>? _serialNumberScannedSubject;

    public override string Path => _settings.GkgTunnelComPort;

    public GkgUutSenderService(
        ILogger logger,
        ISfcService sfcService,
        SerialPortRx serialPortRx,
        SerialScanner serialScanner,
        Settings settings,
        SfcResponseBuilder sfcResponseBuilder,
        UnitUnderTestBuilder unitUnderTestBuilder)
        : base(logger, sfcService, settings, sfcResponseBuilder)
    {
        this._logger = logger;
        this._serialPortRx = serialPortRx;
        this._serialScanner = serialScanner;
        this._settings = settings;
        this._sfcResponseBuilder = sfcResponseBuilder;
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    protected override void StartService()
    {
        this._serialPortRx.PortName = _settings.GkgTunnelComPort;
        this._serialPortRx.Open();
        this._serialScanner.Open();
    }

    protected override void StopService()
    {
        this._serialPortRx.Close();
        this._serialScanner.Close();
    }

    public override Task ReSend(UnitUnderTest unitUnderTest)
    {
        this._serialNumberScannedSubject?.OnNext(unitUnderTest.SerialNumber);
        return Task.CompletedTask;
    }

    public override bool CanReSend(UnitUnderTest unitUnderTest)
    {
        return !string.IsNullOrEmpty(unitUnderTest.SerialNumber);
    }

    protected override void SetupReactiveExtensions()
    {
        this._serialNumberScannedSubject = new Subject<string>();
        this.SerialNumberReceived()
            .Subscribe(serialNumber => this._serialNumberScannedSubject.OnNext(serialNumber))
            .AddTo(ref Disposables);

        this.ValidSerialNumberReceived(this._serialNumberScannedSubject)
            .SelectAwait(async (serialNumber, _) =>
            {
                await this.SendUnitUnderTest(serialNumber);
                return serialNumber;
            })
            .Subscribe(
                _ => { },
                _ => this.Stop())
            .AddTo(ref Disposables);

        this.InvalidSerialNumberReceived(this._serialNumberScannedSubject)
            .Subscribe(
                _ => this.SendScanErrorUnitUnderTest(),
                _ => this.Stop())
            .AddTo(ref Disposables);

        this.DummySerialNumberReceived(this._serialNumberScannedSubject)
            .Do(_ => this.IsWaitingForDummy = false)
            .Subscribe(_ => this.SendDummyUnitUnderTest())
            .AddTo(ref Disposables);

        this._serialScanner
            .State
            .Where(x => x == StateType.Stopped)
            .Subscribe(_ => this.Stop())
            .AddTo(ref Disposables);
    }

    private Observable<string> DummySerialNumberReceived(Subject<string> serialNumberScannedSubject)
    {
        return serialNumberScannedSubject
            .Where(serialNumber => serialNumber == SerialScanner.DummySerialNumber)
            .Do(_ => _logger.Debug($"Dummy enabled"));
    }

    private Observable<string> ValidSerialNumberReceived(Subject<string> serialNumberScannedSubject)
    {
        return serialNumberScannedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(serialNumber => !string.IsNullOrEmpty(serialNumber));
    }

    private Observable<string> InvalidSerialNumberReceived(Subject<string> serialNumberScannedSubject)
    {
        return serialNumberScannedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(string.IsNullOrEmpty);
    }

    private Observable<string> SerialNumberReceived()
    {
        return this._serialPortRx
            .DataReceived
            .Where(x => x.Contains(SerialScanner.TriggerCommand))
            .Do(_ => _logger.Debug($"Trigger received"))
            .SelectAwait(async (_, __) => await ScanSerialNumber())
            .Do(_ => _logger.Debug("Serial number scanned"));
    }

    private Task<string> ScanSerialNumber()
    {
        return Observable
            .FromAsync(async _ =>
            {
                this.State.Value = StateType.Scanning;
                return await this._serialScanner.Scan();
            })
            .Select(x => x.Replace("ERROR", ""))
            .Select(x => x.Trim())
            .Select(x =>
            {
                if (this.IsWaitingForDummy)
                {
                    return string.IsNullOrEmpty(x) ? SerialScanner.DummySerialNumber : x;
                }

                return x;
            }).FirstAsync();
    }

    private async Task SendUnitUnderTest(string serialNumber)
    {
        this.State.Value = StateType.Processing;

        var unitUnderTest = this.BuildUnitUnderTest(serialNumber);
        var sfcResponse = await this.SendUnitUnderTest(unitUnderTest);
        if (sfcResponse.IsOk)
        {
            _serialPortRx.Write($"{serialNumber}{SerialScanner.LineTerminator}");
        }

        _logger.Debug($"Responded to trigger");

        unitUnderTest.SfcResponse = sfcResponse;
        this.UnitUnderTest.Value = unitUnderTest;
        this.State.Value = StateType.Idle;
    }

    private void SendScanErrorUnitUnderTest()
    {
        var unitUnderTest = this.BuildUnitUnderTest(UnitUnderTestBuilder.ScanErrorSerialNumber);
        unitUnderTest.SfcResponse = this._sfcResponseBuilder
            .SetScanError()
            .Build();
        this.UnitUnderTest.Value = unitUnderTest;
        this.State.Value = StateType.Idle;
    }

    private void SendDummyUnitUnderTest()
    {
        this.State.Value = StateType.Processing;

        var unitUnderTest = this._unitUnderTestBuilder
            .Clone()
            .CreatedAt(DateTime.Now)
            .FileNameWithoutExtension($"{UnitUnderTestBuilder.DummySerialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(UnitUnderTestBuilder.DummySerialNumber)
            .Build();

        unitUnderTest.SfcResponse = this._sfcResponseBuilder
            .SetOkSfcResponse()
            .Build();

        this._serialPortRx.Write($"{UnitUnderTestBuilder.DummySerialNumber}{SerialScanner.LineTerminator}");
        this.UnitUnderTest.Value = unitUnderTest;
        this.State.Value = StateType.Idle;
    }

    private UnitUnderTest BuildUnitUnderTest(string serialNumber)
    {
        return this._unitUnderTestBuilder
            .Clone()
            .CreatedAt(DateTime.Now)
            .FileNameWithoutExtension($"{serialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(serialNumber)
            .Build();
    }
}