using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Models;

namespace Hermes.Common.Messages;

public class GeneralSettingsUpdateMessage(GeneralSettings generalSettings)
    : ValueChangedMessage<GeneralSettings>(generalSettings);