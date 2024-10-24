using System.IO;

namespace Hermes.Models;

public class TextDocument
{
    public static TextDocument Null { get; } = new();

    public string Content { get; init; } = "";
    public string FullPath { get; init; } = "";
    public string FileName => Path.GetFileName(this.FullPath);
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(this.FullPath);
    public bool IsEmpty => string.IsNullOrEmpty(this.Content);
    public bool IsNull => this == Null;
}