using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public class GeneralSettingsRepositoryTests
{
    private readonly GeneralSettingsRepository _sut;
    private readonly GeneralSettings _generalSettings;

    public GeneralSettingsRepositoryTests(AesEncryptor aesEncryptor)
    {
        _generalSettings = new GeneralSettings();
        _sut = new GeneralSettingsRepository(aesEncryptor);
    }

    [Fact]
    public void Save_ValidGeneralSettings_WritesFile()
    {
        _sut.Save(_generalSettings);
        Assert.True(File.Exists(_sut.Path));
    }

    [Fact]
    public void Read_ValidGeneralSettings_ReadsFile()
    {
        _sut.Save(_generalSettings);
        var loadedSettings = _sut.Read();
        Assert.Equivalent(_generalSettings, loadedSettings);
    }
}