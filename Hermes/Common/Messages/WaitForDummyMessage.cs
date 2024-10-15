using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class WaitForDummyMessage(bool value) : ValueChangedMessage<bool>(value);