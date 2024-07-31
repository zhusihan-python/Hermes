using Hermes.Types;
using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class Defect
{
    public static readonly Defect Null = new DefectNull();

    [Key] public int Id { get; init; }
    public int UnitUnderTestId { get; init; }
    public ErrorFlag ErrorFlag { get; init; }
    [MaxLength(100)] public string Location { get; init; } = "";
    [MaxLength(100)] public string ErrorCode { get; init; } = "";
    public bool IsNull => this == Null;
}

public class DefectNull : Defect
{
}