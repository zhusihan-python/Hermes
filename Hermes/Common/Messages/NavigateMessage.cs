using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Features;

namespace Hermes.Common.Messages;

public class NavigateMessage(PageBase pageBase) : ValueChangedMessage<PageBase>(pageBase);