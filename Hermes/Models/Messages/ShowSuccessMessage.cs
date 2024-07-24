using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Models.Messages;

public class ShowSuccessMessage(SfcResponse sfcResponse) : ValueChangedMessage<SfcResponse>(sfcResponse);