using Hermes.Models;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class FileService
{
    private const int NumberOfRetries = 3;
    private const int DelayOnRetry = 1000;

    public virtual async Task<string> TryReadAllTextAsync(string fullPath)
    {
        return await Retry(async () => await ReadAllTextAsync(fullPath));
    }

    private static async Task<string> ReadAllTextAsync(string fileFullPath)
    {
        await using var s = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
        using var tr = new StreamReader(s);
        var txt = await tr.ReadToEndAsync();
        return txt;
    }

    public virtual async Task<string> MoveToBackup(string fullPath)
    {
        var fileName = GetFileName(fullPath);
        var backupFullPath = Path.Combine(Settings.Instance.BackupPath, fileName);
        if (File.Exists(backupFullPath))
        {
            File.Delete(backupFullPath);
        }

        return await TryMove(fullPath, backupFullPath);
    }

    private static string GetFileName(string fullPath)
    {
        return $"{Path.GetFileNameWithoutExtension(fullPath)}_{DateTime.Now:dd_MM_HHmmss}{Path.GetExtension(fullPath)}";
    }

    private static async Task<string> TryMove(string fullPath, string backupFullPath)
    {
        return await Retry(() =>
        {
            File.Move(fullPath, backupFullPath);
            return backupFullPath;
        });
    }

    private static async Task<T> Retry<T>(Func<T> func)
    {
        for (var i = 0; i < NumberOfRetries; ++i)
        {
            try
            {
                return func.Invoke();
            }
            catch
            {
                await Task.Delay(DelayOnRetry);
            }
        }

        return default!;
    }

    private static async Task<T> Retry<T>(Func<Task<T>> func)
    {
        for (var i = 0; i < NumberOfRetries; ++i)
        {
            try
            {
                return await func.Invoke();
            }
            catch
            {
                await Task.Delay(DelayOnRetry);
            }
        }

        return default!;
    }

    public virtual async Task WriteAllTextAsync(string path, string content)
    {
        await File.WriteAllTextAsync(path, content);
    }

    public virtual string FileName(string fullPath)
    {
        return Path.GetFileName(fullPath);
    }

    public virtual bool FileExists(string fullPath)
    {
        return File.Exists(fullPath);
    }
}