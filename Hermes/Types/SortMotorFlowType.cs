namespace Hermes.Types;

public enum SortMotorFlowType : byte
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

    // 取出玻片相关状态
    TakingSlide = 0x04,             // 取出玻片中
    TakeSlideSuccess = 0x14,        // 取出玻片成功
    TakeSlideFailed = 0x24,         // 取出玻片失败

    // 放置玻片相关状态
    PlacingSlide = 0x05,            // 放置玻片中
    PlaceSlideSuccess = 0x15,       // 放置玻片成功
    PlaceSlideFailed = 0x25,        // 放置玻片失败

    // 扫描玻片相关状态
    ScanningSlide = 0x06,           // 扫描玻片中
    ScanSlideSuccess = 0x16,        // 扫描玻片成功
    ScanSlideFailed = 0x26,         // 扫描玻片失败

    // 扫描二维码相关状态
    ScanningQrCode = 0x07,          // 扫描二维码中
    ScanQrCodeSuccess = 0x17,       // 扫描二维码成功
    ScanQrCodeFailed = 0x27,        // 扫描二维码失败

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

public static class SortMotorFlowTypeExtensions
{
    public static string GetDescription(this SortMotorFlowType flowType)
    {
        return flowType switch
        {
            SortMotorFlowType.Idle => "空闲",
            SortMotorFlowType.MovingInDirection => "指定方向移动中",
            SortMotorFlowType.MoveInDirectionSuccess => "指定方向移动成功",
            SortMotorFlowType.MoveInDirectionFailed => "指定方向移动失败",
            SortMotorFlowType.Resetting => "复位中",
            SortMotorFlowType.ResetSuccess => "复位成功",
            SortMotorFlowType.ResetFailed => "复位失败",
            SortMotorFlowType.MovingToPosition => "运动到指定位置中",
            SortMotorFlowType.MoveToPositionSuccess => "运动到指定位置成功",
            SortMotorFlowType.MoveToPositionFailed => "运动到指定位置失败",
            SortMotorFlowType.TakingSlide => "取出玻片中",
            SortMotorFlowType.TakeSlideSuccess => "取出玻片成功",
            SortMotorFlowType.TakeSlideFailed => "取出玻片失败",
            SortMotorFlowType.PlacingSlide => "放置玻片中",
            SortMotorFlowType.PlaceSlideSuccess => "放置玻片成功",
            SortMotorFlowType.PlaceSlideFailed => "放置玻片失败",
            SortMotorFlowType.ScanningSlide => "扫描玻片中",
            SortMotorFlowType.ScanSlideSuccess => "扫描玻片成功",
            SortMotorFlowType.ScanSlideFailed => "扫描玻片失败",
            SortMotorFlowType.ScanningQrCode => "扫描二维码中",
            SortMotorFlowType.ScanQrCodeSuccess => "扫描二维码成功",
            SortMotorFlowType.ScanQrCodeFailed => "扫描二维码失败",
            SortMotorFlowType.StoppingSingleMotor => "单个电机停止中",
            SortMotorFlowType.StopSingleMotorSuccess => "单个电机停止完成",
            SortMotorFlowType.StopSingleMotorFailed => "单个电机停止失败",
            SortMotorFlowType.MovingSingleMotor => "单个电机移动中",
            SortMotorFlowType.MoveSingleMotorSuccess => "单个电机移动完成",
            SortMotorFlowType.MoveSingleMotorFailed => "单个电机移动失败",
            SortMotorFlowType.ResettingSingleMotor => "单个电机复位中",
            SortMotorFlowType.ResetSingleMotorSuccess => "单个电机复位完成",
            SortMotorFlowType.ResetSingleMotorFailed => "单个电机复位失败",
            _ => "未知状态"
        };
    }

    public static bool IsProcessing(this SortMotorFlowType? flowType)
    {
        return flowType == SortMotorFlowType.MovingInDirection || 
               flowType == SortMotorFlowType.MovingToPosition || 
               flowType == SortMotorFlowType.TakingSlide || 
               flowType == SortMotorFlowType.PlacingSlide;
    }
}
