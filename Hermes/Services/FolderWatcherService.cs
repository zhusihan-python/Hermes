using Hermes.Common.Reactive;
using Hermes.Models;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class FolderWatcherService : IDisposable
{
    public IObservable<TextDocument> TextDocumentCreated { get; private set; } = null!;

    private readonly FileService _fileService;
    private readonly FileSystemWatcherRx _fileSystemWatcherRx;

    public FolderWatcherService(FileService fileService, FileSystemWatcherRx fileSystemWatcherRx)
    {
        this._fileService = fileService;
        this._fileSystemWatcherRx = fileSystemWatcherRx;
        this.SetupReactiveExtensions();
    }

    private void SetupReactiveExtensions()
    {
        this.TextDocumentCreated = this._fileSystemWatcherRx
            .Created
            .Delay(TimeSpan.FromMilliseconds(10))
            .SelectMany(this.ReadTextDocument);
    }

    private async Task<TextDocument> ReadTextDocument(FileSystemEventArgs e)
    {
        return new TextDocument()
        {
            FullPath = e.FullPath,
            Content = await _fileService.TryReadAllTextAsync(e.FullPath)
        };
    }

    public void Start(string path, string filter = "*.*")
    {
        this._fileSystemWatcherRx.Start(path, filter);
    }

    public void Stop()
    {
        this._fileSystemWatcherRx.Stop();
    }

    public void Dispose()
    {
        this._fileSystemWatcherRx.Dispose();
    }
}