using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

internal class ShowDetailMessage() : ValueChangedMessage<bool>(true);