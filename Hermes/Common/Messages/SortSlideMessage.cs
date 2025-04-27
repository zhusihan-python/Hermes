using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class SortSlideMessage(int option) : ValueChangedMessage<int>(option);