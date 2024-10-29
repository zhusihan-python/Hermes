using Hermes.Common.Extensions;
using Hermes.Repositories;
using Polly.Retry;
using Polly;
using System.IO;
using System.Threading.Tasks;
using System;
using Hermes.Models;

namespace Hermes.Services;

public class FileService
{
    private const string BackupPrefix = "_backupAt_";

    private readonly Settings _settings;
    private readonly ResiliencePipeline _retryPipeline;

    public FileService(Settings settings)
    {
        this._settings = settings;
        this._retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions() { MaxDelay = TimeSpan.FromMilliseconds(50), MaxRetryAttempts = 3 })
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    public virtual async Task<string> TryReadAllTextAsync(string fullPath)
    {
        if (!FileExists(fullPath))
        {
            return string.Empty;
        }

        return await _retryPipeline.ExecuteAsync(async (cancellationToken) =>
        {
            await using var s = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
            using var tr = new StreamReader(s);
            var txt = await tr.ReadToEndAsync(cancellationToken);
            return txt;
        });
    }

    public async Task<string> MoveToBackupAsync(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            return "";
        }

        var backupFullPath = Path.Combine(
            this._settings.BackupPath,
            $"{Path.GetFileNameWithoutExtension(fullPath)}{BackupPrefix}{DateTime.Now:dd_MM_HHmmss}{Path.GetExtension(fullPath)}");
        if (File.Exists(backupFullPath))
        {
            File.Delete(backupFullPath);
        }

        return Path.GetFullPath(await TryMove(fullPath, backupFullPath));
    }

    public async Task<string> CopyFromBackupToInputAsync(string backupFullPath)
    {
        var inputFullPath =
            Path.Combine(this._settings.InputPath, GetFileNameWithoutCurrentDate(backupFullPath));
        if (File.Exists(inputFullPath))
        {
            return inputFullPath;
        }

        return await TryCopy(backupFullPath, inputFullPath);
    }

    private static string GetFileNameWithoutCurrentDate(string fullPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(fullPath);
        var index = fileName.IndexOf(BackupPrefix, StringComparison.OrdinalIgnoreCase);

        if (index != -1)
            fileName = string.Concat(fileName.AsSpan(0, index), Path.GetExtension(fullPath));
        return fileName;
    }

    private async Task<string> TryCopy(string source, string dest)
    {
        return await _retryPipeline.ExecuteAsync((_) =>
        {
            CreateDirectoryIfNotExists(dest);
            File.Copy(source, dest);
            return ValueTask.FromResult(dest);
        });
    }

    private async Task<string> TryMove(string source, string dest)
    {
        return await _retryPipeline.ExecuteAsync((_) =>
        {
            CreateDirectoryIfNotExists(dest);
            File.Move(source, dest);
            return ValueTask.FromResult(dest);
        });
    }

    public async Task<string> DeleteFileIfExists(string fullPath)
    {
        return await _retryPipeline.ExecuteAsync((_) =>
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return ValueTask.FromResult(fullPath);
        });
    }

    public void DeleteFolderIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public virtual async Task WriteAllTextToInputPathAsync(string fileNameWithoutExtension, string content)
    {
        var path = Path.Combine(this._settings.InputPath,
            fileNameWithoutExtension + _settings.InputFileExtension.GetDescription());
        await WriteAllTextAsync(path, content);
    }

    public virtual async Task WriteSfcResponseAsync(string fileNameWithoutExtension, string content)
    {
        var path = Path.Combine(this._settings.SfcPath,
            fileNameWithoutExtension + _settings.SfcResponseExtension.GetDescription());
        await WriteAllTextAsync(path, content);
    }

    public virtual async Task WriteAllTextAsync(string path, string content)
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

    public virtual string FileName(string fullPath)
    {
        return Path.GetFileName(fullPath);
    }

    public virtual bool FileExists(string fullPath)
    {
        return File.Exists(fullPath);
    }
}