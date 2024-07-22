using Hermes.Models.Parsers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class UnitUnderTest
{
    [Key] public int Id { get; set; }
    public string FileName { get; init; }
    public string Content { get; init; }
    public bool IsFail { get; init; }
    public string SerialNumber { get; init; } = string.Empty;
    public List<Defect> Defects { get; init; } = [];

    public UnitUnderTest(string fileName, string content, IUnitUnderTestParser parser) : this(fileName, content)
    {
        this.IsFail = parser.ParseIsFail(content);
        this.SerialNumber = parser.ParseSerialNumber(content);
        this.Defects = parser.ParseDefects(content);
    }

    public UnitUnderTest(string fileName, string content)
    {
        this.FileName = fileName;
        this.Content = content;
        this.FileName = fileName;
    }
}