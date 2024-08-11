using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;
using System.Threading.Tasks;
using System;
using Hermes.Common.Extensions;
using Hermes.Language;
using Hermes.Repositories;

namespace Hermes.Features.UutProcessor;

public partial class UutProcessorViewModel : PageBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private string _stateText = "";
    private readonly Session _session;
    private readonly StopService _stopService;
    private readonly UutSenderService _uutSenderService;
    private readonly ISettingsRepository _settingsRepository;

    public UutProcessorViewModel(
        Session session,
        StopService stopService,
        UutSenderService uutSenderService,
        ISettingsRepository settingsRepository)
        : base(Resources.txt_uut_processor, MaterialIconKind.FolderEye, PermissionLevel.Level5, 1)
    {
        this._session = session;
        this._stopService = stopService;
        this._uutSenderService = uutSenderService;
        this._settingsRepository = settingsRepository;
        this.Path = this._settingsRepository.Settings.InputPath;
        this.IsActive = true;
        this.OnUutProcessorStateChanged(UutProcessorState.Stopped);
    }

    protected override void OnActivated()
    {
        this._session.UutProcessorStateChanged += OnUutProcessorStateChanged;
        this._uutSenderService.UnitUnderTestCreated += OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponse += OnSfcResponse;
        this._uutSenderService.RunStatusChanged += OnSfcSenderRunStatusChanged;
        this._settingsRepository.SettingsChanged += OnSettingsChanged;
        Messenger.Register<StartUutProcessorMessage>(this, this.OnStartReceive);
        Messenger.Register<UnblockMessage>(this, this.OnUnblockReceive);
        Messenger.Register<ExitMessage>(this, this.OnExitReceive);
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        this._uutSenderService.UnitUnderTestCreated -= OnUnitUnderTestCreated;
        this._uutSenderService.SfcResponse -= OnSfcResponse;
        this._uutSenderService.RunStatusChanged -= OnSfcSenderRunStatusChanged;
        this._settingsRepository.SettingsChanged -= OnSettingsChanged;
        Messenger.UnregisterAll(this);
    }

    [RelayCommand]
    private void Start()
    {
        try
        {
            if (this.IsRunning) return;
            this._uutSenderService.Start();
            this._stopService.Start();
            this.IsRunning = true;
            Messenger.Send(new ShowToastMessage("Info", "UUT Processor started"));
        }
        catch (Exception e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                Messenger.Send(new ShowToastMessage("Error in UUT Processor ", e.Message));
            });
        }
    }

    [RelayCommand]
    public void Stop()
    {
        if (!this.IsRunning) return;
        this._uutSenderService.Stop();
        this._stopService.Stop();
        this.IsRunning = false;
        this.SerialNumber = string.Empty;
        this.Path = _settingsRepository.Settings.InputPath;
        Messenger.Send(new ShowToastMessage("Info", "UUT Processor stopped"));
    }

    private void OnUnitUnderTestCreated(object? sender, UnitUnderTest unitUnderTest)
    {
        this.SerialNumber = unitUnderTest.SerialNumber;
        this._session.UutProcessorState = UutProcessorState.Processing;
    }

    private void OnSfcResponse(object? sender, UnitUnderTest unitUnderTest)
    {
        Task.Run(async () =>
        {
            var stop = await this._stopService.Calculate(unitUnderTest);
            if (stop.IsNull)
            {
                Messenger.Send(new ShowSuccessMessage(unitUnderTest));
            }
            else
            {
                _session.Stop = stop;
                this._session.UutProcessorState = UutProcessorState.Blocked;
                Messenger.Send(new ShowStopMessage(stop));
                await this.WaitUntilUnblocked();
                _session.ResetStop();
            }

            this.SerialNumber = string.Empty;
            this._session.UutProcessorState = UutProcessorState.Idle;
        });
    }

    private async Task WaitUntilUnblocked()
    {
        while (this._session.IsUutProcessorBlocked)
        {
            await Task.Delay(100);
        }
    }

    private void OnSfcSenderRunStatusChanged(object? sender, bool isRunning)
    {
        this._session.UutProcessorState = isRunning ? UutProcessorState.Idle : UutProcessorState.Stopped;
        if (!isRunning)
        {
            this.Stop();
        }
    }

    private void OnStartReceive(object recipient, StartUutProcessorMessage message)
    {
        this.Start();
    }

    private void OnExitReceive(object recipient, ExitMessage message)
    {
        this.Stop();
    }

    private void OnUutProcessorStateChanged(UutProcessorState uutProcessorState)
    {
        this.StateText = uutProcessorState.ToTranslatedString();
    }


    private void OnUnblockReceive(object recipient, UnblockMessage message)
    {
        this._session.UutProcessorState = UutProcessorState.Idle;
    }

    private void OnSettingsChanged(Settings settings)
    {
        if (!this.IsRunning)
        {
            this.Path = settings.InputPath;
        }
    }
}