namespace Hermes.Types;

public enum StateType
{
    Stopped,
    Idle,
    Scanning,
    Processing,
    Blocked
}

static class StateTypeExtensions
{
    public static bool IsRunning(this StateType state)
    {
        return state is StateType.Processing or StateType.Scanning or StateType.Idle;
    }
}