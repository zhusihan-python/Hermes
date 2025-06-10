using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class PauseActionMessage() : ValueChangedMessage<bool>(true);