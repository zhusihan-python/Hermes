using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Services;
using Hermes.Types;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Reactive.Disposables;
using R3;

namespace Hermes.Features.UutProcessor;

public partial class ScannerViewModel : ViewModelBase
{
    public ReactiveProperty<string> ScannedText { get; }
    public ReactiveProperty<StateType> State { get; }

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _statusText = StateType.Stopped.ToTranslatedString();
    [ObservableProperty] private string _comPort;

    private readonly ILogger _logger;
    private readonly SerialScanner _serialScanner;

    public ScannerViewModel(
        ILogger logger,
        SerialScanner serialScanner)
    {
        this._logger = logger;
        this._serialScanner = serialScanner;
        this.ComPort = serialScanner.PortName;
        this.ScannedText = _serialScanner
            .ScannedText
            .Select(x => string.IsNullOrEmpty(x) ? "SCAN_ERROR" : x)
            .Select(x => x.Trim())
            .ToBindableReactiveProperty<string>();
        this.State = _serialScanner
            .State
            .ToBindableReactiveProperty();
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {
        this._serialScanner
            .State
            .Subscribe(x => IsConnected = x != StateType.Stopped)
            .AddTo(ref Disposables);

        this._serialScanner
            .ScannedText
            .Subscribe(x => _logger.Debug("Scanned: " + x))
            .AddTo(ref Disposables);
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