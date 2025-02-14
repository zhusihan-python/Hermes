﻿using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using System.Reactive.Disposables;
using R3;

namespace Hermes.Features;

public abstract class ViewModelBase : ObservableRecipient
{
    protected bool SetupReactiveExtensionsOnActivation { get; init; } = true;
    protected DisposableBag Disposables;

    protected override void OnActivated()
    {
        base.OnActivated();
        if (this.SetupReactiveExtensionsOnActivation)
        {
            this.SetupReactiveExtensions();
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        this.Disposables.Clear();
    }

    protected virtual void SetupReactiveExtensions()
    {
    }

    protected void ShowErrorToast(string message, string? title = null)
    {
        Messenger.Send(new ShowToastMessage(title ?? Resources.txt_error, message, NotificationType.Error));
    }

    protected void ShowSuccessToast(string message, string? title = null)
    {
        Messenger.Send(new ShowToastMessage(title ?? Resources.txt_success, message, NotificationType.Success));
    }

    protected void ShowWarningToast(string message, string? title = null)
    {
        Messenger.Send(new ShowToastMessage(title ?? Resources.txt_warning, message, NotificationType.Warning));
    }

    protected void ShowInfoToast(string message, string? title = null)
    {
        Messenger.Send(new ShowToastMessage(title ?? Resources.txt_info, message, NotificationType.Information));
    }
}