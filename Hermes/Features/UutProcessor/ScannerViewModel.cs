using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Services;
using Hermes.Types;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Reactive.Disposables;

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
        this.ComPort = serialScanner.PortName;
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {
        this._serialScanner
            .State
            .ObserveOn(SynchronizationContext.Current!)
            .Do(OnSerialScannerStateChanged)
            .Subscribe()
            .DisposeWith(Disposables);

        this._serialScanner
            .ScannedText
            .ObserveOn(SynchronizationContext.Current!)
            .Do(OnSerialScannerScanned)
            .Subscribe()
            .DisposeWith(Disposables);
    }

    private void OnSerialScannerScanned(string scannedText)
    {
        this.ScannedText = string.IsNullOrEmpty(scannedText) ? "SCAN_ERROR" : scannedText;
        _logger.Debug("Scanned: " + scannedText);
    }

    private void OnSerialScannerStateChanged(StateType state)
    {
        StatusText = state.ToTranslatedString();
        IsConnected = state != StateType.Stopped;
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