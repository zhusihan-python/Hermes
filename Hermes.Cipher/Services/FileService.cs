using System.Text.Json;

namespace Hermes.Cipher.Services;

public class FileService
{
    public static T? ReadJsonEncrypted<T>(string path)
    {
        try
        {
            using var fileStream = File.OpenRead(path);
            using var reader = new StreamReader(fileStream);
            var data = AesEncryptor.Decrypt(reader.ReadToEnd());
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception e)
        {
            return default;
        }
    }

    public static T? ReadJson<T>(string path)
    {
        try
        {
            using var fileStream = File.OpenRead(path);
            using var reader = new StreamReader(fileStream);
            var data = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception e)
        {
            return default;
        }
    }

    public static async Task WriteJsonEncryptedAsync<T>(string path, T content)
    {
        await WriteAllTextAsync(
            path,
            AesEncryptor.Encrypt(JsonSerializer.Serialize(content)));
    }

    public static async Task WriteJsonAsync<T>(string path, T content)
    {
        await WriteAllTextAsync(path, JsonSerializer.Serialize(content));
    }

    public static async Task WriteAllTextAsync(string path, string content)
    {
        CreateDirectoryIfNotExists(path);
        await File.WriteAllTextAsync(path, content);
    }

    private static void CreateDirectoryIfNotExists(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public static bool FileExists(string fullPath)
    {
        return File.Exists(fullPath);
    }

    public static void DeleteFile(string fullpath)
    {
        if (File.Exists(fullpath))
        {
            File.Delete(fullpath);
        }
    }
}