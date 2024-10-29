using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.Models;

namespace Hermes.Common.Messages;

public class ReSendUnitUnderTestMessage(UnitUnderTest unitUnderTest)
    : ValueChangedMessage<UnitUnderTest>(unitUnderTest);