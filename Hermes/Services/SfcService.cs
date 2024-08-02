using Hermes.Models;
using Hermes.Repositories;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hermes.Services;

public class SfcService
{
    private readonly Settings _settings;
    private readonly Stopwatch _stopwatch;
    private readonly FileService _fileService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;
    private readonly SfcResponseRepository _sfcResponseRepository;

    public SfcService(
        Settings settings,
        FileService fileService,
        UnitUnderTestRepository unitUnderTestRepository,
        SfcResponseRepository sfcResponseRepository)
    {
        this._settings = settings;
        this._fileService = fileService;
        this._unitUnderTestRepository = unitUnderTestRepository;
        this._sfcResponseRepository = sfcResponseRepository;
        this._stopwatch = new Stopwatch();
    }

    public async Task<SfcResponse> SendAsync(UnitUnderTest unitUnderTest)
    {
        await this._unitUnderTestRepository.AddAndSaveAsync(unitUnderTest);
        var sfcRequest = new SfcRequest(unitUnderTest, this._settings.SfcPath, this._settings.SfcResponseExtension);
        await this._fileService.WriteAllTextAsync(sfcRequest.FullPath, sfcRequest.Content);

        var sfcResponse = SfcResponse.BuildTimeout(unitUnderTest);
        if (await WaitForFileCreationAsync(sfcRequest.ResponseFullPath))
        {
            sfcResponse = new SfcResponse(
                unitUnderTest,
                content: await this._fileService.TryReadAllTextAsync(sfcRequest.ResponseFullPath)
            );
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