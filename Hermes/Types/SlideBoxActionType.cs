namespace Hermes.Types;

public enum SlideBoxActionType : byte
{
    Idle = 0x00,

    ScanningSlide = 0x02,   // 扫片中
    ScanSlideSuccess = 0x12,// 扫片成功
    ScanSlideFailed = 0x22, // 扫片失败
    ScanSlidePaused = 0xA2, // 扫片暂停
    ScanSlideExited = 0xB2, // 扫片退出

    ScanningCode = 0x03,    // 扫码中
    ScanCodeSuccess = 0x13, // 扫码成功
    ScanCodeFailed = 0x23,  // 扫码失败
    ScanCodePaused = 0xA3,  // 扫码暂停
    ScanCodeExited = 0xB3,  // 扫码退出

    SealingSlide = 0x04,    // 封片中
    SealSlideSuccess = 0x14,// 封片成功
    SealSlideFailed = 0x24, // 封片失败
    SealSlidePaused = 0xA4, // 封片暂停
    SealSlideExited = 0xB4  // 封片退出
}

public static class SlideBoxActionTypeExtensions
{
    public static string GetDescription(this SlideBoxActionType actionType)
    {
        return actionType switch
        {
            SlideBoxActionType.Idle => "空闲",
            SlideBoxActionType.ScanningSlide => "扫片中",
            SlideBoxActionType.ScanSlideSuccess => "扫片成功",
            SlideBoxActionType.ScanSlideFailed => "扫片失败",
            SlideBoxActionType.ScanSlidePaused => "扫片暂停",
            SlideBoxActionType.ScanSlideExited => "扫片退出",
            SlideBoxActionType.ScanningCode => "扫码中",
            SlideBoxActionType.ScanCodeSuccess => "扫码成功",
            SlideBoxActionType.ScanCodeFailed => "扫码失败",
            SlideBoxActionType.ScanCodePaused => "扫码暂停",
            SlideBoxActionType.ScanCodeExited => "扫码退出",
            SlideBoxActionType.SealingSlide => "封片中",
            SlideBoxActionType.SealSlideSuccess => "封片成功",
            SlideBoxActionType.SealSlideFailed => "封片失败",
            SlideBoxActionType.SealSlidePaused => "封片暂停",
            SlideBoxActionType.SealSlideExited => "封片退出",
            _ => "未知状态"
        };
    }
}