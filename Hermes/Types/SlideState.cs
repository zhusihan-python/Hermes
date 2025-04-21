namespace Hermes.Types;

public enum SlideState
{
    Empty,          // 空位置
    NotSorted,      // 待整理
    Sorted,         // 已整理
    NotRecognized,  // 未识别
    SlideBlocked    // 卡片
}

public static class SlideStateExtensions
{
    /// <summary>
    /// 将 bool 值转换为对应的 SlideState 枚举值。
    /// true 转换为 SlideState.NotSorted，false 转换为 SlideState.Empty。
    /// </summary>
    /// <param name="value">要转换的 bool 值。</param>
    /// <returns>对应的 SlideState 枚举值。</returns>
    public static SlideState ToSlideState(this bool value)
    {
        return value ? SlideState.NotSorted : SlideState.Empty;
    }

    public static bool IsEmpty(this SlideState state)
    {
        return state == SlideState.Empty;
    }

    public static bool IsSorted(this SlideState state)
    {
        return state == SlideState.Sorted;
    }

}