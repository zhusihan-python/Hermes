using Hermes.Common;
using System.IO;
using System.Text.Json;
using System;
using Hermes.Models;

namespace Hermes.Repositories;

public class SettingsRepository<T>
{
    public string Path { get; set; }
    public string FileName { get; init; } = "Settings.json";
    private readonly AesEncryptor _aesEncryptor;

    public SettingsRepository(AesEncryptor aesEncryptor)
    {
        this._aesEncryptor = aesEncryptor;
        this.Path = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Hermes",
            FileName);
    }

    public virtual void Save(T settings)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path) ?? string.Empty);
        var jsonData = JsonSerializer.Serialize(settings);
        var encryptedData = _aesEncryptor.Encrypt(jsonData);
        using var fileStream = File.Create(Path);
        using var writer = new StreamWriter(fileStream);
        writer.Write(encryptedData);
    }

    public T? Read()
    {
        if (!File.Exists(Path))
        {
            return default;
        }

        using var fileStream = File.OpenRead(Path);
        using var reader = new StreamReader(fileStream);
        var encryptedData = reader.ReadToEnd();
        var jsonData = _aesEncryptor.Decrypt(encryptedData);
        return JsonSerializer.Deserialize<T>(jsonData);
    }
}