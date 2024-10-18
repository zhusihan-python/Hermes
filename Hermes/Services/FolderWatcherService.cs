using Hermes.Models;
using System.IO;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Hermes.Services;

public class FolderWatcherService
{
    public IObservable<TextDocument> TextDocumentCreated { get; private set; } = null!;

    private FileSystemWatcher _fsw;
    private readonly FileService _fileService;

    public FolderWatcherService(FileService fileService)
    {
        this._fileService = fileService;
        this._fsw = new FileSystemWatcher();
        this.SetupReactiveExtensions();
    }

    private void SetupReactiveExtensions()
    {
        this._fsw = new FileSystemWatcher();

        this.TextDocumentCreated = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                x => _fsw.Created += x,
                x => _fsw.Created -= x)
            .Delay(TimeSpan.FromMilliseconds(20))
            .Select((x) => Observable.FromAsync(async () =>
                {
                    var textDocument = new TextDocument()
                    {
                        FullPath = x.EventArgs.FullPath,
                        Content =
                            await _fileService.TryReadAllTextAsync(x.EventArgs.FullPath)
                    };
                    return textDocument;
                }
            ))
            .Concat();
    }

    public void Start(string path, string filter = "*.*")
    {
        this.ProcessExistingFiles(path);
        this._fsw.Path = path;
        this._fsw.Filter = filter;
        this._fsw.EnableRaisingEvents = true;
    }

    private void ProcessExistingFiles(string path)
    {
        // TODO: implement
        return;
        foreach (var file in Directory.EnumerateFiles(path))
        {
            // this.FileCreated?.Invoke(this, file);
        }
    }

    public void Stop()
    {
        this._fsw.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        this._fsw.Dispose();
    }
}