using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class HideSettingsMessage() : ValueChangedMessage<bool>(true);