using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public class Doctor: ObservableObject
{
    public static readonly Doctor Null = new NullDoctor();
    [Key] public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string Department { get; set; }
    [NotMapped] public bool IsNull => this == Null;

    public class NullDoctor : Doctor { }
}

