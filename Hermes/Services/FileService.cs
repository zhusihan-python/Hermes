using Hermes.Models;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class FileService
{
    private const int NumberOfRetries = 3;
    private const int DelayOnRetry = 1000;

    private readonly Settings _settings;

    public FileService(Settings settings)
    {
        this._settings = settings;
    }

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

    public virtual async Task<string> MoveToBackupAsync(string fullPath)
    {
        var backupFullPath = this.GetBackupFullPath(fullPath);
        if (File.Exists(backupFullPath))
        {
            File.Delete(backupFullPath);
        }

        return await TryMove(fullPath, backupFullPath);
    }

    public async Task<string> CopyToBackupAsync(string fullPath)
    {
        return await Retry(() =>
        {
            var backupFullPath = this.GetBackupFullPath(fullPath);
            File.Move(fullPath, this.GetBackupFullPath(backupFullPath));
            return backupFullPath;
        });
    }

    private string GetBackupFullPath(string fullPath)
    {
        var fileName = GetFileName(fullPath);
        return Path.Combine(this._settings.BackupPath, fileName);
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

    public async Task<string> DeleteIfExists(string fullPath)
    {
        return await Retry(() =>
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return fullPath;
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