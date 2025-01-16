namespace Hermes.Types;

public enum SealMotorResetType : byte
{
    NotRest = 0x00,
    Resetted = 0x01
}

public static class SealMotorResetTypeExtensions
{
    public static string GetDescription(this SealMotorResetType resetType)
    {
        return resetType switch
        {
            SealMotorResetType.NotRest => "封片电机组未复位",
            SealMotorResetType.Resetted => "封片电机组已复位",
            _ => "未知状态"
        };
    }

    public static bool IsResetted(this SealMotorResetType? resetType)
    {
        return resetType == SealMotorResetType.Resetted;
    }
}
