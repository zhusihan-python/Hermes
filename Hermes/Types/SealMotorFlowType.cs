namespace Hermes.Types;

public enum SealMotorFlowType : byte
{
    // 空闲
    Idle = 0x00,

    // 指定方向移动相关状态
    MovingInDirection = 0x01,       // 指定方向移动中
    MoveInDirectionSuccess = 0x11,  // 指定方向移动成功
    MoveInDirectionFailed = 0x21,   // 指定方向移动失败

    // 复位相关状态
    Resetting = 0x02,               // 复位中
    ResetSuccess = 0x12,            // 复位成功
    ResetFailed = 0x22,             // 复位失败

    // 运动到指定位置相关状态
    MovingToPosition = 0x03,        // 运动到指定位置中
    MoveToPositionSuccess = 0x13,   // 运动到指定位置成功
    MoveToPositionFailed = 0x23,    // 运动到指定位置失败

    // 单个电机停止相关状态
    StoppingSingleMotor = 0x0C,     // 单个电机停止中
    StopSingleMotorSuccess = 0x1C,  // 单个电机停止完成
    StopSingleMotorFailed = 0x2C,   // 单个电机停止失败

    // 单个电机移动相关状态
    MovingSingleMotor = 0x0D,       // 单个电机移动中
    MoveSingleMotorSuccess = 0x1D,  // 单个电机移动完成
    MoveSingleMotorFailed = 0x2D,   // 单个电机移动失败

    // 单个电机复位相关状态
    ResettingSingleMotor = 0x0E,    // 单个电机复位中
    ResetSingleMotorSuccess = 0x1E, // 单个电机复位完成
    ResetSingleMotorFailed = 0x2E   // 单个电机复位失败
}

public static class SealMotorFlowTypeExtensions
{
    public static string GetDescription(this SealMotorFlowType flowType)
    {
        return flowType switch
        {
            SealMotorFlowType.Idle => "空闲",
            SealMotorFlowType.MovingInDirection => "指定方向移动中",
            SealMotorFlowType.MoveInDirectionSuccess => "指定方向移动成功",
            SealMotorFlowType.MoveInDirectionFailed => "指定方向移动失败",
            SealMotorFlowType.Resetting => "复位中",
            SealMotorFlowType.ResetSuccess => "复位成功",
            SealMotorFlowType.ResetFailed => "复位失败",
            SealMotorFlowType.MovingToPosition => "运动到指定位置中",
            SealMotorFlowType.MoveToPositionSuccess => "运动到指定位置成功",
            SealMotorFlowType.MoveToPositionFailed => "运动到指定位置失败",
            SealMotorFlowType.StoppingSingleMotor => "单个电机停止中",
            SealMotorFlowType.StopSingleMotorSuccess => "单个电机停止完成",
            SealMotorFlowType.StopSingleMotorFailed => "单个电机停止失败",
            SealMotorFlowType.MovingSingleMotor => "单个电机移动中",
            SealMotorFlowType.MoveSingleMotorSuccess => "单个电机移动完成",
            SealMotorFlowType.MoveSingleMotorFailed => "单个电机移动失败",
            SealMotorFlowType.ResettingSingleMotor => "单个电机复位中",
            SealMotorFlowType.ResetSingleMotorSuccess => "单个电机复位完成",
            SealMotorFlowType.ResetSingleMotorFailed => "单个电机复位失败",
            _ => "未知状态"
        };
    }

    public static bool IsProcessing(this SealMotorFlowType? flowType)
    {
        return flowType == SealMotorFlowType.MovingInDirection || flowType == SealMotorFlowType.MovingToPosition;
    }
}