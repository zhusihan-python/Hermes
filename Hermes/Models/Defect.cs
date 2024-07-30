using Hermes.Types;
using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class Defect
{
    [Key] public int Id { get; init; }
    public int UnitUnderTestId { get; init; }
    public UnitUnderTest UnitUnderTest { get; init; } = UnitUnderTest.Null;
    public ErrorFlag ErrorFlag { get; init; }
    [MaxLength(100)] public string Location { get; init; } = "";
    [MaxLength(100)] public string ErrorCode { get; init; } = "";
}