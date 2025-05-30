using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using Hermes.Types;

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
}
