using Hermes.Communication.Protocol;
using System;

namespace Hermes.Communication.SerialPort;

public class SystemStatusRead : SvtRequestInfo
{
    public SystemStatusRead()
    {
        this.CMDID = Svt.SystemStatus;
        this.FrameType = Svt.Read;
        this.DataLength = 0;
    }
}

public class SystemStatusWrite : SvtRequestInfo
{
    // 0x01：系统复位 0x02：扫片 0x03：扫码 0x04：封片 0x05：执行动作包 0x0A：暂停当前动作 0x0B：退出当前动作
    public SystemStatusWrite()
    {
        this.CMDID = Svt.SystemStatus;
        this.FrameType = Svt.Write;
        this.Data = new byte[76];
        this.DataLength = (ushort)this.Data.Length;
    }

    public SvtRequestInfo WithOperationType(byte operationType)
    {
        this.Data[0] = operationType;
        return this;
    }

    public SvtRequestInfo WithBoxTags(byte[] boxTags)
    {
        boxTags.AsSpan().CopyTo(this.Data.AsSpan(1));
        return this;
    }
}