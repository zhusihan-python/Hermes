using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Hermes.Common.Messages;

public class SlideInfoMessage : ValueChangedMessage<(ushort slideSeq, byte[] originBarcode)>
{
    public SlideInfoMessage((ushort slideSeq, byte[] originBarcode) value) : base(value)
    {
    }
}
