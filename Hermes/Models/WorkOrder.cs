namespace Hermes.Models;

public class WorkOrder
{
    public static readonly WorkOrder Null = new NullWorkOrder();

    public string Id { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string Revision { get; set; } = "";
    public string ModelName { get; set; } = "";
    public bool IsNull => this == Null;
}

public class NullWorkOrder : WorkOrder
{
}