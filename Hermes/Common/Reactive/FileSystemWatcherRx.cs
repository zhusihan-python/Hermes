using System.IO;
using System.Reactive.Linq;
using System;

namespace Hermes.Common.Reactive;

public class FileSystemWatcherRx : IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public IObservable<FileSystemEventArgs> Changed { get; private set; }
    public IObservable<RenamedEventArgs> Renamed { get; private set; }
    public IObservable<FileSystemEventArgs> Deleted { get; private set; }
    public IObservable<ErrorEventArgs> Errors { get; private set; }
    public IObservable<FileSystemEventArgs> Created { get; private set; }

    public FileSystemWatcherRx(FileSystemWatcher watcher)
    {
        _watcher = watcher;

        Changed = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Changed += h,
                h => _watcher.Changed -= h)
            .Select(x => x.EventArgs);

        Renamed = Observable
            .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                h => _watcher.Renamed += h,
                h => _watcher.Renamed -= h)
            .Select(x => x.EventArgs);

        Deleted = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Deleted += h,
                h => _watcher.Deleted -= h)
            .Select(x => x.EventArgs);

        Errors = Observable
            .FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
                h => _watcher.Error += h, h => _watcher.Error -= h)
            .Select(x => x.EventArgs);

        Created = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _watcher.Created += h,
                h => _watcher.Created -= h)
            .Select(x => x.EventArgs);
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