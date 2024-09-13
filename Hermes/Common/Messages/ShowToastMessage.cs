using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SukiUI.Enums;

namespace Hermes.Common.Messages;

public class ShowToastMessage(string title, string message, NotificationType type = NotificationType.Information)
    : ValueChangedMessage<string>(message)
{
    public double Duration { get; set; } = 8;
    public string Title => title;
    public NotificationType Type => type;
};