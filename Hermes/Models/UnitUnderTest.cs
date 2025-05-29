using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Hermes.Types;

namespace Hermes.Models;

public class UnitUnderTest
{
    public static readonly UnitUnderTest Null = new NullUnitTest();

    [Key] public int Id { get; set; }
    [MaxLength(250)] public string SerialNumber { get; init; } = string.Empty;
    public virtual bool IsFail { get; init; }
    public bool IsPass => !IsFail;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Defect> Defects { get; set; } = [];
    public SfcResponse? SfcResponse { get; set; }
    [NotMapped] public string Content { get; init; }
    [MaxLength(512)] public string FullPath { get; set; }
    [NotMapped] public string FileName => Path.GetFileName(this.FullPath);
    [NotMapped] public bool IsNull => this == Null;
    [NotMapped] public bool IsRepair => this.IsFail && this.SfcResponse?.IsFail == false;
    [NotMapped] public bool IsSfcTimeout => this.SfcResponse?.IsTimeout == true;
    [NotMapped] public bool IsSfcFail => this.SfcResponse?.IsFail == true;
    [NotMapped] public string Message { get; set; } = "";

    [NotMapped]
    public string SfcResponseFullPath
    {
        get => this.SfcResponse?.FullPath ?? "";
        set
        {
            if (SfcResponse != null) SfcResponse.FullPath = value;
        }
    }

    public UnitUnderTest(string fullPath, string content)
    {
        this.FullPath = fullPath;
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
            if (string.IsNullOrEmpty(additionalOkSfcResponse)) return false;
            return SfcResponse != null && SfcResponse.Content.Contains(additionalOkSfcResponse);
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public class NullUnitTest() : UnitUnderTest(string.Empty, string.Empty);