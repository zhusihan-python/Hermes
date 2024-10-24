using R3;
using System.IO;
using System;

namespace Hermes.Common.Reactive;

public class FileSystemWatcherRx : IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public Observable<FileSystemEventArgs> Changed { get; private set; }
    public Observable<RenamedEventArgs> Renamed { get; private set; }
    public Observable<FileSystemEventArgs> Deleted { get; private set; }
    public Observable<ErrorEventArgs> Errors { get; private set; }
    public Observable<FileSystemEventArgs> Created { get; private set; }

    public FileSystemWatcherRx(FileSystemWatcher watcher)
    {
        _watcher = watcher;

        Changed = Observable
            .FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h => _watcher.Changed += h,
                h => _watcher.Changed -= h);

        Renamed = Observable
            .FromEvent<RenamedEventHandler, RenamedEventArgs>(
                h => (sender, e) => h(e),
                h => _watcher.Renamed += h,
                h => _watcher.Renamed -= h);

        Deleted = Observable
            .FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h => _watcher.Deleted += h,
                h => _watcher.Deleted -= h);

        Errors = Observable
            .FromEvent<ErrorEventHandler, ErrorEventArgs>(
                h => (sender, e) => h(e),
                h => _watcher.Error += h, h => _watcher.Error -= h);

        Created = Observable
            .FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h => _watcher.Created += h,
                h => _watcher.Created -= h);
    }

    public FileSystemWatcherRx()
        : this(new FileSystemWatcher())
    {
    }

    public void Start(string? path = null, string filter = "*.*")
    {
        if (path != null)
        {
            _watcher.Path = path;
        }

        _watcher.Filter = filter;
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}