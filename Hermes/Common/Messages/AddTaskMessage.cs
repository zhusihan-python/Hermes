using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class AddTaskMessage(DistributedTask task) : ValueChangedMessage<DistributedTask>(task);