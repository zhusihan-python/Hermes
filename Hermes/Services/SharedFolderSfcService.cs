using Hermes.Cipher.Extensions;
using Hermes.Models;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System;

namespace Hermes.Services;

public class SharedFolderSfcService : ISfcService
{
    private readonly FileService _fileService;
    private readonly FolderWatcherService _folderWatcherService;
    private readonly Settings _settings;

    public SharedFolderSfcService(
        FileService fileService,
        FolderWatcherService folderWatcherService,
        Settings settings)
    {
        this._settings = settings;
        this._folderWatcherService = folderWatcherService;
        this._fileService = fileService;
    }


    public Task<SfcResponse> SendAsync(UnitUnderTest unitUnderTest)
    {
        this._folderWatcherService.Start(
            this._settings.SfcPath);

        var responseCreated = this._folderWatcherService
            .TextDocumentCreated
            .Delay(TimeSpan.FromMilliseconds(20))
            .Timeout(TimeSpan.FromSeconds(this._settings.SfcTimeoutSeconds))
            .Where(x => IsResponseFile(x, unitUnderTest))
            .Select(GetResponseFileContent)
            .Concat()
            .Take(1);

        return Observable
            .FromAsync(_ => SendUnitUnderTest(unitUnderTest))
            .Select(_ => responseCreated)
            .Concat()
            .Select(x => new SfcResponse(
                content: x.Content,
                fullPath: x.FullPath,
                additionalOkResponse: _settings.AdditionalOkSfcResponse
            ))
            .Catch<SfcResponse, TimeoutException>(_ => Observable.Return(SfcResponse.BuildTimeout()))
            .Catch<SfcResponse, Exception>(_ => Observable.Return(SfcResponse.Null))
            .Finally(() => this._folderWatcherService.Stop())
            .ToTask();
    }

    private bool IsResponseFile(TextDocument textDocument, UnitUnderTest unitUnderTest)
    {
        return unitUnderTest.FileName.StartsWith(textDocument.FileNameWithoutExtension) &&
               !textDocument.FileName.EndsWith(this._settings.InputFileExtension.GetDescription());
    }

    private async Task<TextDocument> GetResponseFileContent(TextDocument textDocument)
    {
        var responseFullPath = Path.Combine(Path.GetPathRoot(textDocument.FullPath)!,
            textDocument.FileNameWithoutExtension +
            this._settings.SfcResponseExtension.GetDescription());
        return new TextDocument()
        {
            FullPath = responseFullPath,
            Content = await this._fileService.TryReadAllTextAsync(responseFullPath)
        };
    }

    private async Task SendUnitUnderTest(UnitUnderTest unitUnderTest)
    {
        await this._fileService.WriteAllTextAsync(
            Path.Combine(this._settings.SfcPath, unitUnderTest.FileName),
            unitUnderTest.Content);
    }
}