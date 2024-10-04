using System.ComponentModel;

namespace Hermes.Types;

public enum StationType
{
    Labeling,
    LabelingMachine,
    [Description("21")] ScreenPrinterBottom,
    [Description("00")] SpiBottom,
    [Description("01")] Aoi1,
    [Description("02")] Aoi2,
    [Description("03")] SpiTop,
    [Description("22")] ScreenPrinterTop,
    [Description("04")] Aoi3,
    [Description("05")] Aoi4,
    [Description("07")] Pth,
    [Description("08")] Axi,
    None = 99
}