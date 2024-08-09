using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Moq;
using NLog.Config;

namespace HermesTests.Services;

public class FileServiceMockBuilder
{
    private bool _fileExists = true;
    private string _tryReadAllTextAsync = String.Empty;
    private Settings _settings = new();

    public FileServiceMockBuilder FileExists(bool fileExists)
    {
        this._fileExists = true;
        return this;
    }

    public FileServiceMockBuilder TryReadAllTextAsync(string content)
    {
        this._tryReadAllTextAsync = content;
        return this;
    }

    public FileService Build()
    {
        var settingsRepositoryMock = new Mock<SettingsRepository>(new AesEncryptor());
        settingsRepositoryMock.Setup(x => x.Settings)
            .Returns(new Settings());
        var fileServiceMock = new Mock<FileService>(settingsRepositoryMock.Object);
        fileServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(this._fileExists);
        fileServiceMock.Setup(x => x.TryReadAllTextAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(this._tryReadAllTextAsync));
        return fileServiceMock.Object;
    }
}