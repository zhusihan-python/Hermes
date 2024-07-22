using Hermes.Types;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Hermes.Models;

public class SfcRequest
{
    [Key] public int Id { get; init; }
    public UnitUnderTest UnitUnderTest { get; set; }
    public int UnitUnderTestId { get; init; }
    public string ResponseFullPath { get; init; }
    public string FullPath { get; init; }
    public string Content => this.UnitUnderTest.Content;

    public SfcRequest(UnitUnderTest unitUnderTest, string path, SfcResponseExtension sfcResponseExtension)
    {
        this.UnitUnderTest = unitUnderTest;
        this.FullPath = Path.Combine(path, unitUnderTest.FileName);
        this.ResponseFullPath = Path.Combine(path,
            $"{Path.GetFileNameWithoutExtension(unitUnderTest.FileName)}.{sfcResponseExtension.ToString().ToLower()}");
    }
}