using Hermes.Communication.Protocol;

namespace Hermes.Communication.SerialPort;

public class HeartBeatRead : SvtRequestInfo
{
    public HeartBeatRead()
    {
        this.CMDID = Svt.HeartBeat;
        this.FrameType = Svt.Read;
        this.DataLength = 0;
    }
}
