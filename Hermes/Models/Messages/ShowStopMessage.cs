using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Models.Messages;

public class ShowStopMessage(Stop stop) : ValueChangedMessage<Stop>(stop);