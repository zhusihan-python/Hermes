using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Models.Messages;

public class ShowSnackbarMessage(string message) : ValueChangedMessage<string>(message);