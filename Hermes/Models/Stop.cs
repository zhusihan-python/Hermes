using Hermes.Cipher.Extensions;
using Hermes.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
    [NotMapped] public string Details { get; set; } = "";
    [NotMapped] public string SerialNumber { get; set; } = "";
    [NotMapped] public bool IsFake { get; init; }

    private string? _message;

    [NotMapped]
    public string? Message
    {
        get => _message ?? $"Stop {this.Type.ToTranslatedString()}";
        set => this._message = value;
    }

    public Stop()
    {
    }

    public Stop(StopType stopType)
    {
        this.Type = stopType;
    }
}