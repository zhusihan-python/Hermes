using System.Buffers.Binary;
using System;
using TouchSocket.Core;
using System.Linq;

namespace Hermes.Communication.SerialPort;

public class ScanRequestInfo : IRequestInfo, IRequestInfoBuilder
{
    public byte[] header;
    public byte[] dataFrame;
    public byte[] tail;
    public byte[] FrameNo { get; set; }
    public ushort SlideSeq { get; set; }
    public ushort DataLength { get; set; }
    public int MaxLength => this.DataLength;

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

    public byte[] CalculateChecksum(byte[] byteArray)
    {
        // 求和 按位取反 加 1
        ushort checksum = (ushort)(~byteArray.Sum(b => (int)b) + 1);

        byte[] checkSumArray = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(checkSumArray.AsSpan(0, 2), checksum);

        return checkSumArray;
    }
}

public class ScanStartRequest : ScanRequestInfo
{
    public ScanStartRequest(ushort sequence, byte[] frameNo)
    {
        this.dataFrame = new byte[] { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
        this.DataLength = (ushort)this.dataFrame.Length;
        this.SlideSeq = sequence;
        this.FrameNo = frameNo;
    }
}