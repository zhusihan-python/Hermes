using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Models;

namespace Hermes.Common.Messages;

public class ShowSuccessMessage(UnitUnderTest unitUnderTest) : ValueChangedMessage<UnitUnderTest>(unitUnderTest);