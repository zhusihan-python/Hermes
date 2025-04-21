using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class HeartBeatMessage(bool value) : ValueChangedMessage<bool>(value);
