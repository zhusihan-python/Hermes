namespace Hermes.Types;

public enum SortMotorFlowType : byte
{
    Idle = 0x00,            // 空闲
    Resetting = 0x01,       // 复位中
    ResetSuccess = 0x11,    // 复位成功
    ResetFailed = 0x21,     // 复位失败
    Processing = 0x02,      // 理片中
    ProcessSuccess = 0x12,  // 理片成功
    ProcessFailed = 0x22,   // 理片失败
    Scanning = 0x03,        // 玻片扫码中
    ScanSuccess = 0x13,     // 玻片扫码成功
    ScanFailed = 0x23       // 玻片扫码失败
}

public static class SortMotorFlowTypeExtensions
{
    public static string GetDescription(this SortMotorFlowType flowType)
    {
        return flowType switch
        {
            SortMotorFlowType.Idle => "空闲",
            SortMotorFlowType.Resetting => "理片电机组复位中",
            SortMotorFlowType.ResetSuccess => "理片电机组复位成功",
            SortMotorFlowType.ResetFailed => "理片电机组复位失败",
            SortMotorFlowType.Processing => "理片中",
            SortMotorFlowType.ProcessSuccess => "理片成功",
            SortMotorFlowType.ProcessFailed => "理片失败",
            SortMotorFlowType.Scanning => "玻片扫码中",
            SortMotorFlowType.ScanSuccess => "玻片扫码成功",
            SortMotorFlowType.ScanFailed => "玻片扫码失败",
            _ => "未知状态"
        };
    }

    public static bool IsProcessing(this SortMotorFlowType? flowType)
    {
        return flowType == SortMotorFlowType.Processing;
    }
}
