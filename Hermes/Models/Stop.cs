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
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsMachineStop => this.Type == StopType.Machine;
    [NotMapped] public string SerialNumber => this.SfcResponse?.SerialNumber ?? "";

    [NotMapped]
    public string Message
    {
        get => GetMessage().ToUpper();
        set => _message = value;
    }

    [NotMapped]
    public string Details
    {
        get => GetDetails();
        set => _details = value;
    }

    private string? _message;
    private string? _details;

    public Stop()
    {
    }

    public Stop(StopType stopType, SfcResponse? sfcResponse)
    {
        this.Type = stopType;
        this.SfcResponse = sfcResponse;
    }

    private string GetDetails()
    {
        if (string.IsNullOrWhiteSpace(_details) && SfcResponse != null)
        {
            return SfcResponse.Details;
        }

        return _details ?? this.Message;
    }

    private string GetMessage()
    {
        return !string.IsNullOrWhiteSpace(_message) ? _message : $"Stop {Type}";
    }
}