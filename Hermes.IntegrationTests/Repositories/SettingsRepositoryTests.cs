using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;

namespace HermesIntegrationTests.Repositories;

public class SettingsRepositoryTests
{
    private readonly SettingsRepository _sut = new();
    private readonly Settings _settings = new();

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