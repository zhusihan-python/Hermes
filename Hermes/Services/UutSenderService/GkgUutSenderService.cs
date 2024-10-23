using Hermes.Builders;
using Hermes.Common.Reactive;
using Hermes.Common;
using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;
using System;
using R3;

namespace Hermes.Services.UutSenderService;

public class GkgUutSenderService : UutSenderService
{
    private readonly ILogger _logger;
    private readonly SerialScanner _serialScanner;
    private readonly SerialPortRx _serialPortRx;
    private readonly Settings _settings;
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private DisposableBag _disposables;

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

    private void SetupReactiveExtensions()
    {
        var serialNumberReceivedSubject = new Subject<string>();
        TriggerReceived()
            .Select(async _ => await ScanSerialNumber())
            .Do(_ => _logger.Debug("Serial number scanned"))
            .SubscribeAwait(async (serialNumber, _) => serialNumberReceivedSubject.OnNext(await serialNumber))
            .AddTo(ref _disposables);

        serialNumberReceivedSubject
            .Where(serialNumber => serialNumber == SerialScanner.DummySerialNumber)
            .Do(_ => _logger.Debug($"Dummy enabled"))
            .Do(_ => this.IsWaitingForDummy = false)
            .Subscribe(_ => this.SendDummyUnitUnderTest())
            .AddTo(ref _disposables);

        serialNumberReceivedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(serialNumber => !string.IsNullOrEmpty(serialNumber))
            .Select(async serialNumber =>
            {
                _logger.Debug($"Send serial number: {serialNumber}");
                await this.SendSerialNumber(serialNumber);
                return serialNumber;
            })
            .Subscribe(
                _ => { },
                _ => this.Stop())
            .AddTo(ref _disposables);

        serialNumberReceivedSubject
            .Where(serialNumber => serialNumber != SerialScanner.DummySerialNumber)
            .Where(string.IsNullOrEmpty)
            .Subscribe(
                _ => this.SendScanErrorUnitUnderTest(),
                _ => this.Stop())
            .AddTo(ref _disposables);

        this._serialScanner
            .State
            .Do(x =>
            {
                if (x == StateType.Stopped)
                {
                    this.Stop();
                }
            })
            .Subscribe()
            .AddTo(ref _disposables);
    }

    private Observable<string> TriggerReceived()
    {
        return this._serialPortRx
            .DataReceived
            .Where(x => x.Contains(SerialScanner.TriggerCommand))
            .Do(_ => _logger.Debug($"Trigger received"))
            .ThrottleFirst(TimeSpan.FromSeconds(5));
    }

    private Task<string> ScanSerialNumber()
    {
        return Observable
            .FromAsync(async (_) =>
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

    private async Task SendSerialNumber(string serialNumber)
    {
        this.State.Value = StateType.Processing;

        var unitUnderTest = this.BuildUnitUnderTest(serialNumber);
        this.UnitUnderTestCreated.Value = unitUnderTest;
        var sfcResponse = await this.SendUnitUnderTest(unitUnderTest);
        if (sfcResponse.IsOk)
        {
            _serialPortRx.Write($"{serialNumber}{SerialScanner.LineTerminator}");
        }

        _logger.Debug($"Responded to trigger");
        this.SfcResponseCreated.Value = sfcResponse;
        this.State.Value = StateType.Idle;
    }

    private void SendScanErrorUnitUnderTest()
    {
        this.SfcResponseCreated.Value = this._sfcResponseBuilder
            .SetScanError()
            .Build();
        this.State.Value = StateType.Idle;
    }

    protected override void StartService()
    {
        this._serialPortRx.PortName = _settings.GkgTunnelComPort;
        this._serialPortRx.Open();
        this._serialScanner.Open();
        this.SetupReactiveExtensions();
    }

    private void SendDummyUnitUnderTest()
    {
        this.State.Value = StateType.Processing;

        var unitUnderTest = this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{UnitUnderTestBuilder.DummySerialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(UnitUnderTestBuilder.DummySerialNumber)
            .Build();
        this.UnitUnderTestCreated.Value = unitUnderTest;

        this.SfcResponseCreated.Value = this._sfcResponseBuilder
            .SetOkSfcResponse()
            .Build();

        _serialPortRx.Write($"{UnitUnderTestBuilder.DummySerialNumber}{SerialScanner.LineTerminator}");

        this.State.Value = StateType.Idle;
    }

    private UnitUnderTest BuildUnitUnderTest(string serialNumber)
    {
        return this._unitUnderTestBuilder
            .Clone()
            .FileNameWithoutExtension($"{serialNumber}_{DateTime.Now:yyMMddHHmmss}")
            .SerialNumber(serialNumber)
            .Build();
    }

    protected override void StopService()
    {
        this._serialPortRx.Close();
        this._serialScanner.Close();
    }
}