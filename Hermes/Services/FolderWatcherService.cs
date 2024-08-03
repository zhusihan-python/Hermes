using Hermes.Models;
using System.IO;
using System;

namespace Hermes.Services;

public class FolderWatcherService
{
    public string Filter { get; set; } = "*.*";
    public event EventHandler<string>? FileCreated;

    private FileSystemWatcher? _watcher;

    public void Start(string path)
    {
        this.ProcessExistingFiles(path);
        this._watcher = new FileSystemWatcher(path);
        this._watcher.Filter = this.Filter;
        this._watcher.Created += this.OnFileCreated;
        this._watcher.EnableRaisingEvents = true;
    }

    private void ProcessExistingFiles(string path)
    {
        foreach (var file in Directory.EnumerateFiles(path))
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