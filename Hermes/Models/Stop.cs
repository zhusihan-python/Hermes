using Hermes.Types;
using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class Stop
{
    public static readonly Stop Null = new();

    [Key] public int Id { get; init; }
    public SfcResponse? SfcResponse { get; set; }
    public int SfcResponseId { get; init; }
    public StopType Type { get; init; } = StopType.None;
    public bool IsRestored { get; set; }
    public bool IsNull => this == Null;
    public bool IsMachineStop => this.Type == StopType.Machine;
    public string LogfileSerialNumber => this.SfcResponse?.SerialNumber ?? "";
    public string Details => $"{Type} - {SfcResponse?.Details ?? ""}";

    public Stop()
    {
    }

    public Stop(StopType stopType, SfcResponse? logfile)
    {
        this.Type = stopType;
        this.SfcResponse = logfile;
    }
}