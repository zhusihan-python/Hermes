namespace Hermes.Types;

public enum SortMotorResetType : byte
{
    NotRest = 0x00,
    Resetted = 0x01
}

public static class SortMotorResetTypeExtensions
{
    public static string GetDescription(this SortMotorResetType resetType)
    {
        return resetType switch
        {
            SortMotorResetType.NotRest => "理电机组未复位",
            SortMotorResetType.Resetted => "理片电机组已复位",
            _ => "未知状态"
        };
    }

    public static bool IsResetted(this SortMotorResetType? resetType)
    {
        return resetType == SortMotorResetType.Resetted;
    }
}