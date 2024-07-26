using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Models.Messages;

public class ExitMessage() : ValueChangedMessage<bool>(true);