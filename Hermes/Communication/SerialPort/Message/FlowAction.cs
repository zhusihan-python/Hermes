using System;
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
    public byte[] ActionSquence = { 0x00, 0x01 };
    public byte[] SlideSequence = { 0x00, 0x01 };
    public byte ActionType = 0x01; // 0x01：封片 0x02：理片 0x03：封片后理片
    public byte PickCount = 0x01; // 玻片取放次数
    public byte[] SourceOne = { 0x00, 0x01 };
    public byte[] DestinationOne = { 0x00, 0x01 };
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

    public SvtRequestInfo WithOperationType(byte operationType)
    {
        this.OperationType = operationType;
        return this;
    }

    public SvtRequestInfo WithActionSequence(byte[] actionSequence)
    {
        this.ActionSquence = actionSequence;
        return this;
    }

    public SvtRequestInfo WithSlideSequence(byte[] slideSequence)
    {
        this.SlideSequence = slideSequence;
        return this;
    }

    public SvtRequestInfo WithActionType(byte actionType)
    {
        this.ActionType = actionType;
        return this;
    }

    public SvtRequestInfo WithPickCount(byte pickCount)
    {
        this.PickCount = pickCount;
        return this;
    }

    public SvtRequestInfo WithSrcDstLocations(byte[] srcDstLocations)
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
}
