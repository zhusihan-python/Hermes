using Hermes.Builders;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Hermes.Services;

public class GkgUutSenderService : UutSenderService
{
    private SerialPort? _serialPort;
    private readonly SerialScanner _serialScanner;

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
        try
        {
            var instruction = _serialPort?.ReadExisting() ?? string.Empty;
            if (!instruction.StartsWith(SerialScanner.TriggerCommand)) return;

            UnitUnderTest uut;
            var serialNumber = await _serialScanner.Scan();
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

            if (uut.SfcResponse is { IsFail: false })
            {
                _serialPort.WriteLine($"{serialNumber}\n");
            }

            this.OnSfcResponse(uut);
        }
        catch (Exception exception)
        {
            var uut = _unitUnderTestBuilder
                .ResponseFailMessage(exception.Message)
                .Build();
            Logger.Error(exception.Message);
            this.OnSfcResponse(uut);
        }
    }

    public override void Stop()
    {
        if (!IsRunning) return;
        _serialPort?.Dispose();
        this.CancellationTokenSource?.Cancel();
        _serialScanner.Stop();
        this.OnRunStatusChanged(false);
    }
}