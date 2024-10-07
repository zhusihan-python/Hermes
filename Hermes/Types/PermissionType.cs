namespace Hermes.Types;

public enum PermissionType
{
    FreeAccess,
    Exit,
    OpenBender,
    OpenSettingsConfig,
    OpenSfcSimulator,
    OpenUutProcessor,
    OpenUserAdmin,
    RestoreSpiStop,
    RestoreScreenPrinterStop,
    RestoreLabelingMachineStop,
    RestoreAoiStop,
    RestoreAxiStop,
    Admin = 99
}

public static class FeatureTypeExtensions
{
    public static PermissionType GetFeatureType(StationType station)
    {
        switch (station)
        {
            case StationType.LabelingMachine:
                return PermissionType.RestoreLabelingMachineStop;
            case StationType.ScreenPrinterBottom:
            case StationType.ScreenPrinterTop:
                return PermissionType.RestoreScreenPrinterStop;
            case StationType.Aoi1:
            case StationType.Aoi2:
            case StationType.Aoi3:
            case StationType.Aoi4:
                return PermissionType.RestoreAoiStop;
            case StationType.Axi:
                return PermissionType.RestoreAxiStop;
            case StationType.SpiBottom:
            case StationType.SpiTop:
            default:
                return PermissionType.RestoreSpiStop;
        }
    }
}