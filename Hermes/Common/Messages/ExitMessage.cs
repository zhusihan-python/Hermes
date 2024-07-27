using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class ExitMessage() : ValueChangedMessage<bool>(true);