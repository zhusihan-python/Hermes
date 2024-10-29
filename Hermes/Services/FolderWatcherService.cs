using Hermes.Common.Reactive;
using Hermes.Models;
using R3;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Hermes.Services;

public class FolderWatcherService : IDisposable
{
    public virtual Observable<TextDocument> TextDocumentCreated { get; private set; } = null!;

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
            .Delay(TimeSpan.FromMilliseconds(20))
            .SelectAwait(ReadTextDocument);
    }

    private async ValueTask<TextDocument> ReadTextDocument(FileSystemEventArgs e, CancellationToken ct)
    {
        var textDocument = new TextDocument()
        {
            FullPath = e.FullPath,
            Content = await _fileService.TryReadAllTextAsync(e.FullPath)
        };
        return textDocument;
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

    public async Task ResendAsync(UnitUnderTest unitUnderTest)
    {
        await this._fileService.CopyFromBackupToInputAsync(unitUnderTest.FullPath);
    }

    public bool FileExists(string fullPath)
    {
        return this._fileService.FileExists(fullPath);
    }
}