using Hermes.Communication.Protocol;

namespace Hermes.Communication.SerialPort;

public class ScanTriggerWriteResponse : SvtRequestInfo
{
    public ScanTriggerWriteResponse(byte[] frameNumber)
    {
        this.FrameNo = frameNumber;
        this.CMDID = Svt.ScanTrigger;
        this.FrameType = Svt.WriteResponse;
        this.DataLength = 1;
    }

    public SvtRequestInfo TriggerSuccess()
    {
        return WithData<ScanTriggerWriteResponse>(new byte[] { Svt.WriteScueess });
    }

    public SvtRequestInfo TriggerFail()
    {
        return WithData<ScanTriggerWriteResponse>(new byte[] { Svt.WriteFailed });
    }
}

public class ScanResultWrite : SvtRequestInfo
{
    public ScanResultWrite(byte[] frameNumber)
    {
        this.FrameNo = frameNumber;
        this.CMDID = Svt.ScanResult;
        this.FrameType = Svt.Write;
        this.DataLength = 1;
    }

    public SvtRequestInfo ScanSuccess()
    {
        return WithData<ScanResultWrite>(new byte[] { 0x01 });
    }

    public SvtRequestInfo ScanFail()
    {
        return WithData<ScanResultWrite>(new byte[] { 0x02 });
    }
}