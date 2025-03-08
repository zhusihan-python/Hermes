using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hermes.Communication.Protocol;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class FlowActionRead : SvtRequestInfo
{
    public FlowActionRead()
    {
        this.CMDID = Svt.FlowAction;
        this.FrameType = Svt.Read;
    }

    // 0x00：读取步骤总数 1-65535：读取指定步骤

    public SvtRequestInfo WithQuery(ushort QueryType)
    {
        return WithData<FlowActionRead>(BitConverter.GetBytes(QueryType).Reverse().ToArray());
    }
}

public class FlowActionWrite : SvtRequestInfo
{
    public byte OperationType = 0x01; // 0x00：清空所有 0x01：增加一个动作 0x02：写入动作包总数
    public byte[] ActionSquence = { 0x00, 0x00 };
    public byte[] SlideSequence = { 0x00, 0x00 };
    public byte ActionType = 0x01; // 0x01：封片 0x02：理片 0x03：封片后理片
    public byte PickCount = 0x01; // 玻片取放次数
    public byte[] SourceOne = { 0x00, 0x00 };
    public byte[] DestinationOne = { 0x00, 0x00 };
    public byte[] SourceTwo = { 0x00, 0x00 };
    public byte[] DestinationTwo = { 0x00, 0x00 };
    public byte[] SourceThree = { 0x00, 0x00 };
    public byte[] DestinationThree = { 0x00, 0x00 };
    public byte[] SourceFour = { 0x00, 0x00 };
    public byte[] DestinationFour = { 0x00, 0x00 };
    public byte[] SourceFive = { 0x00, 0x00 };
    public byte[] DestinationFive = { 0x00, 0x00 };

    public FlowActionWrite()
    {
        this.CMDID = Svt.FlowAction;
        this.FrameType = Svt.Write;
    }

    public FlowActionWrite WithOperationType(byte operationType)
    {
        this.OperationType = operationType;
        return this;
    }

    public FlowActionWrite WithActionSequence(byte[] actionSequence)
    {
        this.ActionSquence = actionSequence;
        return this;
    }

    public FlowActionWrite WithSlideSequence(byte[] slideSequence)
    {
        this.SlideSequence = slideSequence;
        return this;
    }

    public FlowActionWrite WithActionType(byte actionType)
    {
        this.ActionType = actionType;
        return this;
    }

    public FlowActionWrite WithPickCount(byte pickCount)
    {
        this.PickCount = pickCount;
        return this;
    }

    public FlowActionWrite WithSrcDstLocations(byte[] srcDstLocations)
    {
        if (srcDstLocations == null)
        {  throw new ArgumentNullException(); }
        if (srcDstLocations.Length < 20)
        { throw new ArgumentException("位置数组长度不够"); }

        var chunks = Enumerable.Range(0, 10)
            .Select(i => srcDstLocations.Skip(i * 2).Take(2).ToArray())
            .ToArray();

        SourceOne = chunks[0];
        DestinationOne = chunks[1];
        SourceTwo = chunks[2];
        DestinationTwo = chunks[3];
        SourceThree = chunks[4];
        DestinationThree = chunks[5];
        SourceFour = chunks[6];
        DestinationFour = chunks[7];
        SourceFive = chunks[8];
        DestinationFive = chunks[9];

        return this;
    }

    public FlowActionWrite GenData()
    {
        int totalLength = 1 + this.ActionSquence.Length + this.SlideSequence.Length + 1 + 1 +
                      this.SourceOne.Length + this.DestinationOne.Length +
                      this.SourceTwo.Length + this.DestinationTwo.Length +
                      this.SourceThree.Length + this.DestinationThree.Length +
                      this.SourceFour.Length + this.DestinationFour.Length +
                      this.SourceFive.Length + this.DestinationFive.Length;
        byte[] combinedArray = new byte[totalLength];
        Span<byte> combinedSpan = combinedArray;

        combinedSpan[0] = OperationType;
        combinedSpan = combinedSpan.Slice(1);

        ActionSquence.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(ActionSquence.Length);

        SlideSequence.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SlideSequence.Length);

        combinedSpan[0] = ActionType;
        combinedSpan = combinedSpan.Slice(1);

        combinedSpan[0] = PickCount;
        combinedSpan = combinedSpan.Slice(1);

        SourceOne.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SourceOne.Length);

        DestinationOne.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(DestinationOne.Length);

        SourceTwo.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SourceTwo.Length);

        DestinationTwo.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(DestinationTwo.Length);

        SourceThree.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SourceThree.Length);

        DestinationThree.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(DestinationThree.Length);

        SourceFour.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SourceFour.Length);

        DestinationFour.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(DestinationFour.Length);

        SourceFive.CopyTo(combinedSpan);
        combinedSpan = combinedSpan.Slice(SourceFive.Length);

        DestinationFive.CopyTo(combinedSpan);

        this.WithData<FlowActionWrite>(combinedArray);

        //Debug.WriteLine($"GenData combinedSpan.ToArray(): {string.Join(" ", this.Data.Select(b => b.ToString("X2")))}");
        return this;
    }
}

public class SortWriteBatch
{
    private readonly List<(UInt16, UInt16)> _locations;

    public SortWriteBatch(List<(UInt16, UInt16)> locations)
    {
        _locations = locations ?? throw new ArgumentNullException(nameof(locations));
    }

    public FlowActionWrite[] GenerateMessages()
    {
        if (_locations.Count <= 100)
        {
            return GenerateSortBatchMessages();
        }
        else
        {
            return GenerateLargeBatchMessages();
        }
    }

    private FlowActionWrite[] GenerateSortBatchMessages()
    {
        int messageCount = Math.Min(_locations.Count, 100);
        var messages = new FlowActionWrite[messageCount];

        for (int i = 0; i < messageCount; i++)
        {
            var srcDstLocations = new byte[20];
            var span = new Span<byte>(srcDstLocations);

            BinaryPrimitives.WriteUInt16BigEndian(srcDstLocations.AsSpan(0, 2), _locations[i].Item1);
            BinaryPrimitives.WriteUInt16BigEndian(srcDstLocations.AsSpan(2, 4), _locations[i].Item2);

            var packet = new FlowActionWrite().
                WithMasterAddress<FlowActionWrite>(0xF2).
                WithSlaveAddress<FlowActionWrite>(0x13).
                WithActionSequence(BitConverter.GetBytes(i).Reverse().ToArray()).
                WithActionType(0x02).
                WithPickCount(1).
                WithSrcDstLocations(srcDstLocations);
            messages[i] = packet;
        }
        return messages;
    }

    private FlowActionWrite[] GenerateLargeBatchMessages()
    {
        int messageCount = (_locations.Count + 4) / 5;
        var messages = new FlowActionWrite[messageCount];

        for (int i = 0; i < messageCount; i++)
        {
            var message = new FlowActionWrite
            {
                ActionType = 2,
                PickCount = 5
            };

            int startIndex = i * 5;
            int endIndex = Math.Min(startIndex + 5, _locations.Count);

            for (int j = 0; j < endIndex - startIndex; j++)
            {
                var location = _locations[startIndex + j];
                switch (j)
                {
                    case 0:
                        BinaryPrimitives.WriteUInt16BigEndian(message.SourceOne, location.Item1);
                        BinaryPrimitives.WriteUInt16BigEndian(message.DestinationOne, location.Item2);
                        break;
                    case 1:
                        BinaryPrimitives.WriteUInt16BigEndian(message.SourceTwo, location.Item1);
                        BinaryPrimitives.WriteUInt16BigEndian(message.DestinationTwo, location.Item2);
                        break;
                    case 2:
                        BinaryPrimitives.WriteUInt16BigEndian(message.SourceThree, location.Item1);
                        BinaryPrimitives.WriteUInt16BigEndian(message.DestinationThree, location.Item2);
                        break;
                    case 3:
                        BinaryPrimitives.WriteUInt16BigEndian(message.SourceFour, location.Item1);
                        BinaryPrimitives.WriteUInt16BigEndian(message.DestinationFour, location.Item2);
                        break;
                    case 4:
                        BinaryPrimitives.WriteUInt16BigEndian(message.SourceFive, location.Item1);
                        BinaryPrimitives.WriteUInt16BigEndian(message.DestinationFive, location.Item2);
                        break;
                }
            }
            messages[i] = message;
        }
        return messages;
    }
}