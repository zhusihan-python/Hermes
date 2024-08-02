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

    public SfcRequest(UnitUnderTest unitUnderTest, string sfcInputPath, SfcResponseExtension sfcResponseExtension)
    {
        this.UnitUnderTest = unitUnderTest;
        this.FullPath = Path.Combine(sfcInputPath, unitUnderTest.FileName);
        this.ResponseFullPath = GetResponseFullpath(this.FullPath, sfcResponseExtension);
    }

    public static string GetResponseFullpath(string fullPath, SfcResponseExtension sfcResponseExtension)
    {
        return Path
            .Combine(
                Path.GetDirectoryName(fullPath)!,
                $"{Path.GetFileNameWithoutExtension(fullPath)}.{sfcResponseExtension.ToString().ToLower()}")
            .Replace("\\", "/");
    }
}