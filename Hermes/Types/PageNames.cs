using System;
using System.ComponentModel;
using System.Linq;

namespace Hermes.Types;

public enum PageNames
{
    [Description("未知页面")]
    Unknown,

    [Description("主界面")]
    Home,

    [Description("历史")]
    History,

    [Description("设置")]
    Settings,

    [Description("用户")]
    Login
}

// 获取描述的扩展方法
public static class EnumExtensions
{
    public static string GetPageDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                             .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}