using CommunityToolkit.Mvvm.Messaging.Messages;
using SukiUI.Enums;

namespace Hermes.Common.Messages;

public class ShowToastMessage(string title, string message)
    : ValueChangedMessage<string>(message)
{
    public string Title => title;
};