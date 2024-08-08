using System.ComponentModel;

namespace Hermes.Types;

public enum FileExtension
{
    // TODO: Add custom attribute for extension
    [Description(".ret")]
    Ret,
    [Description(".3dx")]
    _3dx,
    [Description(".log")]
    Log,
    [Description(".rle")]
    Rle,
    [Description(".txt")]
    Txt,
    [Description(".res")]
    Res
}