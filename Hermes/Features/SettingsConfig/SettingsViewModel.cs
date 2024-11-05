using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Language;
using R3;
using SukiUI.Toasts;

namespace Hermes.Features.SettingsConfig;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISukiToastManager _toastManager;
    public SettingsConfigModel Model { get; init; }

    public SettingsViewModel(
        ISukiToastManager toastManager,
        SettingsConfigModel model)
    {
        this._toastManager = toastManager;
        this.Model = model;
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {
        this.Model
            .SettingsSaved
            .Skip(1)
            .Do(x => this.ShowToast())
            .Do(x => Messenger.Send(new HideSettingsMessage()))
            .Subscribe()
            .AddTo(ref this.Disposables);
    }

    private void ShowToast()
    {
        _toastManager.CreateToast()
            .OfType(NotificationType.Warning)
            .WithTitle(Resources.txt_settings_changed)
            .WithContent(Resources.msg_settings_changed_needs_restart)
            .WithActionButtonNormal(Resources.txt_later, _ => { }, true)
            .WithActionButton(Resources.txt_restart, _ => { App.Restart(); }, true)
            .Queue();
    }

    public void Refresh()
    {
        Model.Refresh();
    }
}