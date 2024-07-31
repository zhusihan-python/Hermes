using Hermes.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public class Stop
{
    public static readonly Stop Null = new();

    [Key] public int Id { get; init; }
    public SfcResponse? SfcResponse { get; set; }
    public int SfcResponseId { get; init; }
    public StopType Type { get; init; } = StopType.None;
    public bool IsRestored { get; set; }
    public Defect? Defect { get; set; }
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsMachineStop => this.Type == StopType.Machine;
    [NotMapped] public string SerialNumber => this.SfcResponse?.SerialNumber ?? "";
    [NotMapped] public string Message => GetMessage().ToUpper();
    [NotMapped] public string Details => GetDetails();

    public Stop()
    {
    }

    public Stop(StopType stopType, SfcResponse? sfcResponse)
    {
        this.Type = stopType;
        this.SfcResponse = sfcResponse;
        this.SfcResponseId = sfcResponse?.Id ?? 0;
    }

    private string GetDetails()
    {
        if (SfcResponse?.IsNull == false)
        {
            return SfcResponse.Details;
        }

        return "";
    }

    private string GetMessage()
    {
        if (this.SfcResponse?.IsFail == true)
        {
            return SfcResponse.Content;
        }

        return $"Stop {Type}";
    }
}