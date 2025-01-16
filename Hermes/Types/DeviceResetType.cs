namespace Hermes.Types;

public enum DeviceResetType : byte
{
    NotRest = 0x00,
    Resetted = 0x01
}

public static class DeviceResetTypeExtensions
{
    public static string GetDescription(this DeviceResetType resetType)
    {
        return resetType switch
        {
            DeviceResetType.NotRest => "整机未复位",
            DeviceResetType.Resetted => "整机已复位",
            _ => "未知状态"
        };
    }

    public static bool IsResetted(this DeviceResetType? resetType)
    {
        return resetType == DeviceResetType.Resetted;
    }
}