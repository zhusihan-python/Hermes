using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Types;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public partial class Record : ObservableObject
{
    [MaxLength(32)]
    public string RecordId { get; set; } = string.Empty;
    public RecordType RecordType { get; set; }
    public RecordStatusType RecordStatus { get; set; }
    public int UserId { get; set; }

    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; }
    public class NullRecord : Record { }
    [NotMapped] // Tell EF Core not to map this property to a database column
    public string RecordTypeDisplay => RecordType switch
    {
        RecordType.Seal => "封片",
        RecordType.Sort => "理片",
        RecordType.SealSort => "封片+理片",
    };

    [NotMapped] // Tell EF Core not to map this property to a database column
    public string RecordStatusDisplay => RecordStatus switch
    {
        RecordStatusType.NotFinish => "未完成",
        RecordStatusType.Finished => "已完成",
        RecordStatusType.Abnormal => "异常",
        _ => "未知"
    };
}
