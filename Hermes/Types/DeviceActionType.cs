namespace Hermes.Types;

public enum DeviceActionType : byte
{
    Idle = 0x00,

    Resetting = 0x01,           // 复位中
    ResetSuccess = 0x11,        // 复位成功
    ResetFailed = 0x21,         // 复位失败

    ScanningSlide = 0x02,       // 扫片中
    ScanSlideSuccess = 0x12,    // 扫片成功
    ScanSlideFailed = 0x22,     // 扫片失败
    ScanSlidePaused = 0xA2,     // 扫片暂停
    ScanSlideExited = 0xB2,     // 扫片退出

    ScanningCode = 0x03,        // 扫码中
    ScanCodeSuccess = 0x13,     // 扫码成功
    ScanCodeFailed = 0x23,      // 扫码失败
    ScanCodePaused = 0xA3,      // 扫码暂停
    ScanCodeExited = 0xB3,      // 扫码退出

    SealingSlide = 0x04,        // 封片中
    SealSlideSuccess = 0x14,    // 封片成功
    SealSlideFailed = 0x24,     // 封片失败
    SealSlidePaused = 0xA4,     // 封片暂停
    SealSlideExited = 0xB4,     // 封片退出

    ExecutingActionPack = 0x05, // 动作包执行中
    ActionPackCompleted = 0x15, // 动作包执行完成
    ActionPackFailed = 0x25,    // 动作包执行失败
    ActionPackPaused = 0xA5,    // 动作包执行暂停
    ActionPackExited = 0xB5     // 动作包执行退出
}

public static class DeviceActionTypeExtensions
{
    public static string GetDescription(this DeviceActionType actionType)
    {
        return actionType switch
        {
            DeviceActionType.Idle => "空闲",
            DeviceActionType.Resetting => "复位中",
            DeviceActionType.ResetSuccess => "复位成功",
            DeviceActionType.ResetFailed => "复位失败",
            DeviceActionType.ScanningSlide => "扫片中",
            DeviceActionType.ScanSlideSuccess => "扫片成功",
            DeviceActionType.ScanSlideFailed => "扫片失败",
            DeviceActionType.ScanSlidePaused => "扫片暂停",
            DeviceActionType.ScanSlideExited => "扫片退出",
            DeviceActionType.ScanningCode => "扫码中",
            DeviceActionType.ScanCodeSuccess => "扫码成功",
            DeviceActionType.ScanCodeFailed => "扫码失败",
            DeviceActionType.ScanCodePaused => "扫码暂停",
            DeviceActionType.ScanCodeExited => "扫码退出",
            DeviceActionType.SealingSlide => "封片中",
            DeviceActionType.SealSlideSuccess => "封片成功",
            DeviceActionType.SealSlideFailed => "封片失败",
            DeviceActionType.SealSlidePaused => "封片暂停",
            DeviceActionType.SealSlideExited => "封片退出",
            DeviceActionType.ExecutingActionPack => "动作包执行中",
            DeviceActionType.ActionPackCompleted => "动作包执行完成",
            DeviceActionType.ActionPackFailed => "动作包执行失败",
            DeviceActionType.ActionPackPaused => "动作包执行暂停",
            DeviceActionType.ActionPackExited => "动作包执行退出",
            _ => "未知状态"
        };
    }
}
