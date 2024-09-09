using System;
using Hermes.Common.Extensions;

namespace Hermes.Models;

public class Package
{
    public static readonly Package Null = new NullPackage();

    public string Id { get; set; } = "-";
    public int Quantity { get; set; }
    public int QuantityUsed { get; set; }
    public DateTime Opened { get; set; }
    public string DateCode { get; set; } = "-";
    public string Lot { get; set; } = "-";
    public string Vendor { get; set; } = "-";
    public string WorkOrder { get; set; } = "-";
    public bool IsNull => this == Null;

    public static string NormalizePkgId(string pkgid)
    {
        return pkgid.ExtractFirstLetter('S');
    }

    public static string NormalizeWorkOrder(string workOrder)
    {
        return workOrder.PadLeft(12, '0');
    }
}

public class NullPackage : Package
{
}