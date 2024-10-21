using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Services;
using Hermes.Types;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UutProcessor;

public partial class ScannerViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _statusText = StateType.Stopped.ToTranslatedString();
    [ObservableProperty] private string _comPort;
    [ObservableProperty] private string _scannedText = "";

    private readonly ILogger _logger;
    private readonly SerialScanner _serialScanner;

    public ScannerViewModel(
        ILogger logger,
        SerialScanner serialScanner)
    {
        this._logger = logger;
        this._serialScanner = serialScanner;
        this._serialScanner.StateChanged += OnSerialScannerStateChanged;
        this._serialScanner.Scanned += OnSerialScannerScanned;
        this.ComPort = serialScanner.PortName;
    }

    private void OnSerialScannerScanned(string scannedText)
    {
        this.ScannedText = string.IsNullOrEmpty(scannedText) ? "SCAN_ERROR" : scannedText;
    }

    private void OnSerialScannerStateChanged(StateType state)
    {
        StatusText = state.ToTranslatedString();
        IsConnected = state != StateType.Stopped;
        if (state == StateType.Processing)
        {
            this.ScannedText = "";
        }
    }

    [RelayCommand]
    private void Start()
    {
        try
        {
            _serialScanner.Open();
            this.ComPort = _serialScanner.PortName;
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    [RelayCommand]
    private void Stop()
    {
        try
        {
            _serialScanner.Close();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }

    [RelayCommand]
    private async Task Scan()
    {
        await _serialScanner.Scan();
    }
}