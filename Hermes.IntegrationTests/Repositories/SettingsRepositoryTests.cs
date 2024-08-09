using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public class SettingsRepositoryTests
{
    private readonly SettingsRepository _sut;
    private readonly Settings _settings;

    public SettingsRepositoryTests(AesEncryptor aesEncryptor)
    {
        _settings = new Settings();
        _sut = new SettingsRepository(aesEncryptor);
    }

    [Fact]
    public void Save_ValidGeneralSettings_WritesFile()
    {
        _sut.Save(_settings);
        Assert.True(File.Exists(_sut.Path));
    }

    [Fact]
    public void Read_ValidGeneralSettings_ReadsFile()
    {
        _sut.Save(_settings);
        var loadedSettings = _sut.Load();
        Assert.Equivalent(_settings, loadedSettings);
    }
}