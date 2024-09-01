using System;

namespace Hermes.Models;

public class Package
{
    public string Id { get; set; } = "";
    public int Quantity { get; set; }
    public int QuantityUsed { get; set; }
    public DateTime Opened { get; set; }
}