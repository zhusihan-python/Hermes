using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hermes.Types;

namespace Hermes.Models;

public class UnitUnderTest
{
    public static readonly UnitUnderTest Null = new NullUnitTest();

    [Key] public int Id { get; set; }
    [MaxLength(250)] public string FileName { get; init; }
    [MaxLength(250)] public string SerialNumber { get; init; } = string.Empty;
    public virtual bool IsFail { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Defect> Defects { get; set; } = [];
    public Stop? Stop { get; set; }
    public SfcResponse? SfcResponse { get; set; }
    [NotMapped] public string Content { get; init; }
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsRepair => this.IsFail && this.SfcResponse?.IsFail == false;
    [NotMapped] public bool IsSfcTimeout => this.SfcResponse?.IsTimeout == true;
    [NotMapped] public bool IsSfcFail => this.SfcResponse?.IsFail == true;
    [NotMapped] public string Message { get; set; } = "";

    public UnitUnderTest(string fileName, string content)
    {
        this.FileName = fileName;
        this.Content = content;
    }

    public UnitUnderTest() : this(string.Empty, string.Empty)
    {
    }

    public virtual Defect GetDefectByLocation(string criticalLocations)
    {
        return Defects
            .Where(x => x.ErrorFlag == ErrorFlag.Bad)
            .Where(x => criticalLocations.Contains(x.Location, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(Defect.Null);
    }

    public bool SfcResponseContains(string additionalOkSfcResponse)
    {
        try
        {
            return SfcResponse != null && SfcResponse.Content.Contains(additionalOkSfcResponse);
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public class NullUnitTest() : UnitUnderTest(string.Empty, string.Empty);