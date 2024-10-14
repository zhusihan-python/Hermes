using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Aspects;
using Hermes.Common.Extensions;
using Hermes.Services;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Features.UutProcessor;

public partial class ScannerViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _statusText = StateType.Stopped.ToTranslatedString();
    [ObservableProperty] private string _comPort;
    [ObservableProperty] private string _scannedText = "";
    private readonly SerialScanner _serialScanner;

    public ScannerViewModel(SerialScanner serialScanner)
    {
        this._serialScanner = serialScanner;
        this._serialScanner.StateChanged += OnSerialScannerStateChanged;
        this._serialScanner.Scanned += OnSerialScannerScanned;
        this.ComPort = serialScanner.PortName;
    }

    private void OnSerialScannerScanned(string scannedText)
    {
        this.ScannedText = scannedText;
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
    [CatchExceptionAndShowErrorToast]
    private void Start()
    {
        _serialScanner.Start();
        this.ComPort = _serialScanner.PortName;
    }

    [RelayCommand]
    [CatchExceptionAndShowErrorToast]
    private void Stop()
    {
        _serialScanner.Stop();
    }

    [RelayCommand]
    private async Task Scan()
    {
        await _serialScanner.Scan();
    }
}