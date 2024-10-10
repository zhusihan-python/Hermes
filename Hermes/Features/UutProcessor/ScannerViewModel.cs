using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Services;
using Hermes.Types;

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
    private void Start()
    {
        try
        {
            _serialScanner.Start();
            this.ComPort = _serialScanner.PortName;
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", e.Message, NotificationType.Error));
        }
    }

    [RelayCommand]
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