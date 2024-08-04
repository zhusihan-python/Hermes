using Hermes.Models;
using Hermes.Repositories;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hermes.Services;

public class SharedFolderSfcService : ISfcService
{
    private readonly Settings _settings;
    private readonly Stopwatch _stopwatch;
    private readonly FileService _fileService;
    private readonly SfcResponseRepository _sfcResponseRepository;

    public SharedFolderSfcService(
        Settings settings,
        FileService fileService,
        SfcResponseRepository sfcResponseRepository)
    {
        this._settings = settings;
        this._fileService = fileService;
        this._sfcResponseRepository = sfcResponseRepository;
        this._stopwatch = new Stopwatch();
    }

    public async Task<SfcResponse> SendAsync(UnitUnderTest unitUnderTest)
    {
        var sfcRequest = new SfcRequest(unitUnderTest, this._settings.SfcPath, this._settings.SfcResponseExtension);
        await this._fileService.WriteAllTextAsync(sfcRequest.FullPath, sfcRequest.Content);

        var sfcResponse = SfcResponse.BuildTimeout();
        if (await WaitForFileCreationAsync(sfcRequest.ResponseFullPath))
        {
            sfcResponse = new SfcResponse(
                content: await this._fileService.TryReadAllTextAsync(sfcRequest.ResponseFullPath)
            );
            await _fileService.MoveToBackupAsync(sfcRequest.ResponseFullPath);
        }

        await this._sfcResponseRepository.AddAndSaveAsync(sfcResponse);
        return sfcResponse;
    }

    private async Task<bool> WaitForFileCreationAsync(string fullPath)
    {
        var timeout = this._settings.SfcTimeoutSeconds * 1000;
        this._stopwatch.Restart();
        while (!this._fileService.FileExists(fullPath) && this._stopwatch.ElapsedMilliseconds <= timeout)
        {
            await Task.Delay(this._settings.WaitDelayMilliseconds);
        }

        this._stopwatch.Stop();
        return this._stopwatch.ElapsedMilliseconds < timeout;
    }
}