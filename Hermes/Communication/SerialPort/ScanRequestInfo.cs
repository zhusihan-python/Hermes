using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class ScanRequestInfo : IRequestInfo, IRequestInfoBuilder
{
    public byte[] header;
    public byte[] dataFrame;
    public byte[] tail;
    public ushort DataLength { get; set; }
    public int MaxLength => this.DataLength + this.header.Length + this.tail.Length;

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.Write(this.dataFrame);
    }

    public T WithData<T>(byte[] data) where T : ScanRequestInfo
    {
        DataLength = (ushort)data.Length;
        dataFrame = data;
        return (T)this;
    }
}
