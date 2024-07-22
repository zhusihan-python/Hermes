using Hermes.Types;
using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class Defect
{
    [Key] public int Id { get; init; }
    public int LogfileId { get; init; }
    public ErrorFlag ErrorFlag { get; init; }
    [MaxLength(50)] public string Location { get; init; } = "";
    [MaxLength(50)] public string ErrorCode { get; init; } = "";
}