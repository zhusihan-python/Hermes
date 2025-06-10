using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class StopActionMessage() : ValueChangedMessage<bool>(true);