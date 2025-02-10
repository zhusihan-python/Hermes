namespace Hermes.Types;

public enum FlowType
{
    Start,          // 空位置
    Cancel,         // 取消
    Delay,          // 延迟
    Pause,          // 待整理
    Resume,         // 已整理
    Stop,           // 停止
}