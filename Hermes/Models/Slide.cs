using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public partial class Slide : ObservableObject
{
    public static readonly Slide Null = new NullSlide();
    [Key] public int Id { get; set; }

    [MaxLength(64)]
    public string ProgramId { get; set; } = string.Empty;
    public int PathologyId { get; set; }
    [Required]
    [MaxLength(16)]
    public int SlideId { get; set; }
    [MaxLength(64)]
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }  // 外键
    public Doctor? Doctor { get; set; }  // 导航属性
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public byte SealState { get; set; } = 0;
    public byte SortState { get; set; } = 0;
    public class NullSlide : Slide { }
    [NotMapped] // Tell EF Core not to map this property to a database column
    public string SealStateDisplay => SealState == 0 ? "未封片" : "已封片";

    [NotMapped] // Tell EF Core not to map this property to a database column
    public string SortStateDisplay => SortState == 0 ? "未理片" : "已理片";
}
