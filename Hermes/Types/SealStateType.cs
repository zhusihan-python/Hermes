using System.ComponentModel;

namespace Hermes.Types;

public enum SealStateType
{
    [Description("未封片")]
    SlideNotSeal,
    [Description("已封片")]
    SlideSealed,
}
