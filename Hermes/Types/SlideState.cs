﻿namespace Hermes.Types;

public enum SlideState
{
    Empty,          // 空位置
    NotSorted,      // 待整理
    Sorted,         // 已整理
    NotRecognized,  // 未识别
    SlideBlocked    // 卡片
}
