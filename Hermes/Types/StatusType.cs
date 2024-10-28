namespace Hermes.Types;

public enum StatusType
{
    Failed,
    Passed,
    NotRan,
    Unknown
}

public static class StatusTypeExtensions
{
    public static bool IsFail(this StatusType? statusType)
    {
        return statusType == StatusType.Failed;
    }
}