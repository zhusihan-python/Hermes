using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Models;

namespace Hermes.Common.Messages;

public class ShowSuccessMessage(SfcResponse sfcResponse) : ValueChangedMessage<SfcResponse>(sfcResponse);