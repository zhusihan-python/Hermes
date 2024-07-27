using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Models;

namespace Hermes.Common.Messages;

public class ShowStopMessage(Stop stop) : ValueChangedMessage<Stop>(stop);