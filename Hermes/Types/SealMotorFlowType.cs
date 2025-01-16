namespace Hermes.Types;

public enum SealMotorFlowType : byte
{
    Idle = 0x00,            // 空闲
    Resetting = 0x01,       // 复位中
    ResetSuccess = 0x11,    // 复位成功
    ResetFailed = 0x21,     // 复位失败
    Processing = 0x02,      // 封片中
    ProcessSuccess = 0x12,  // 封片成功
    ProcessFailed = 0x22,   // 封片失败
}

public static class SealMotorFlowTypeExtensions
{
    public static string GetDescription(this SealMotorFlowType flowType)
    {
        return flowType switch
        {
            SealMotorFlowType.Idle => "封片电机组空闲",
            SealMotorFlowType.Resetting => "封片电机组复位中",
            SealMotorFlowType.ResetSuccess => "封片电机组复位成功",
            SealMotorFlowType.ResetFailed => "封片电机组复位失败",
            SealMotorFlowType.Processing => "封片中",
            SealMotorFlowType.ProcessSuccess => "封片成功",
            SealMotorFlowType.ProcessFailed => "封片失败",
            _ => "未知状态"
        };
    }

    public static bool IsProcessing(this SealMotorFlowType? flowType)
    {
        return flowType == SealMotorFlowType.Processing;
    }
}