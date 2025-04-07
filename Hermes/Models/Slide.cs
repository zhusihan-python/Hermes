using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static Hermes.Models.Doctor;

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
    public string EntryDate { get; set; } = string.Empty;
    public class NullSlide : Slide { }
}
