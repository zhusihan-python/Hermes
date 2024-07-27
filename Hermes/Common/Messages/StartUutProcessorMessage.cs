using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class StartUutProcessorMessage() : ValueChangedMessage<bool>(true);