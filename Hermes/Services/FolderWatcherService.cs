using System;
using System.IO;
using Hermes.Models;

namespace Hermes.Services;

public class FolderWatcherService
{
    public event EventHandler<string>? FileCreated;

    private FileSystemWatcher? _watcher;
    private readonly Settings _settings;

    public FolderWatcherService(Settings settings)
    {
        this._settings = settings;
    }

    public void Start()
    {
        this.ProcessExistingFiles();
        this._watcher = new FileSystemWatcher(this._settings.InputPath);
        this._watcher.Created += this.OnFileCreated;
        this._watcher.EnableRaisingEvents = true;
    }

    private void ProcessExistingFiles()
    {
        foreach (var file in Directory.EnumerateFiles(this._settings.InputPath))
        {
            this.FileCreated?.Invoke(this, file);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        this.FileCreated?.Invoke(this, e.FullPath);
    }

    public void Stop()
    {
        this._watcher?.Dispose();
    }
}