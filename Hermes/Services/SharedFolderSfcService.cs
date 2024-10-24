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
        // TODO : Verify if this is the correct way to do this
        this._folderWatcherService.Start(this._settings.SfcPath);

        var responseCreated = this._folderWatcherService
            .TextDocumentCreated
            .Timeout(TimeSpan.FromSeconds(this._settings.SfcTimeoutSeconds))
            .Where(x => IsResponseFile(x, unitUnderTest))
            .SelectAwait(async (x, __) => await this.GetResponseFileContent(x))
            .Catch<TextDocument, TimeoutException>(_ => Observable.Return(new TextDocument()))
            .Take(1);

        var asd = Observable
            .FromAsync(async _ => await SendUnitUnderTest(unitUnderTest))
            .Timeout(TimeSpan.FromSeconds(this._settings.SfcTimeoutSeconds))
            .Select(x => new SfcResponse())
            .Catch<SfcResponse, TimeoutException>(_ => Observable.Return(SfcResponse.BuildTimeout()))
            .AsObservable();

        return asd.Zip(responseCreated, (_, x) => x)
            .Timeout(TimeSpan.FromSeconds(this._settings.SfcTimeoutSeconds))
            .Select((x) => new SfcResponse(
                content: x.Content,
                fullPath: x.FullPath,
                additionalOkResponse: _settings.AdditionalOkSfcResponse
            ))
            .Do(() => this._folderWatcherService.Stop())
            .Catch<SfcResponse, TimeoutException>(_ => Observable.Return(SfcResponse.BuildTimeout()))
            .Catch<SfcResponse, Exception>(_ => Observable.Return(SfcResponse.Null))
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