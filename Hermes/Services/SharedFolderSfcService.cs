using Hermes.Cipher.Extensions;
using Hermes.Models;
using R3;
using System.IO;
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
        this._folderWatcherService.Start(this._settings.SfcPath);

        var uutSent = Observable
            .FromAsync(async _ => await SendUnitUnderTest(unitUnderTest))
            .Select(x => TextDocument.Null);

        var responseCreated = this._folderWatcherService
            .TextDocumentCreated
            .Where(x => IsResponseFile(x, unitUnderTest))
            .SelectAwait(async (x, __) => await this.GetResponseFileContent(x))
            .Take(1);

        return uutSent
            .Concat(responseCreated)
            .Timeout(TimeSpan.FromSeconds(this._settings.SfcTimeoutSeconds))
            .Where(x => !x.IsNull)
            .Select((x) => new SfcResponse(
                content: x.Content,
                fullPath: x.FullPath,
                additionalOkResponse: _settings.AdditionalOkSfcResponse
            ))
            .Catch<SfcResponse, TimeoutException>(_ => Observable.Return(SfcResponse.BuildTimeout()))
            .Catch<SfcResponse, Exception>(_ => Observable.Return(SfcResponse.Null))
            .Do(onCompleted: _ => this._folderWatcherService.Stop())
            .LastAsync();
    }

    private bool IsResponseFile(TextDocument textDocument, UnitUnderTest unitUnderTest)
    {
        return unitUnderTest.FileName.StartsWith(textDocument.FileNameWithoutExtension) &&
               !textDocument.FileName.EndsWith(this._settings.InputFileExtension.GetDescription());
    }

    private async Task<TextDocument> GetResponseFileContent(TextDocument textDocument)
    {
        var responseFullPath = Path.Combine(Path.GetDirectoryName(textDocument.FullPath)!,
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