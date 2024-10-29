using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Features;

namespace Hermes.Common.Messages;

public class UnblockMessage() : ValueChangedMessage<bool>(true);
