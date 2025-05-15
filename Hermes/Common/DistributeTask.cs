namespace Hermes.Common;

public class DistributedTask
{
    public string Position { get; set; }
    public double ProgressValue { get; set; }
    public string ActionType { get; set; }

    public DistributedTask(string position, double progressValue, string actionType)
    {
        Position = position;
        ProgressValue = progressValue;
        ActionType = actionType;
    }
}
