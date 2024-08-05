using System.Collections.Generic;
using Hermes.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public class Stop
{
    public static readonly Stop Null = new();

    [Key] public int Id { get; init; }
    public StopType Type { get; set; } = StopType.None;
    public bool IsRestored { get; set; }
    public List<Defect> Defects { get; set; } = [];
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsMachineStop => this.Type == StopType.Machine;
    [NotMapped] public string Message => $"Stop {Type}";
    [NotMapped] public string Details { get; set; } = "";
    [NotMapped] public string SerialNumber { get; set; } = "";
    [NotMapped] public bool IsFake { get; init; }

    public Stop()
    {
    }

    public Stop(StopType stopType)
    {
        this.Type = stopType;
    }
}