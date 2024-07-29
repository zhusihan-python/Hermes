using Hermes.Models;
using Hermes.Services;
using Moq;

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

    public FileServiceMockBuilder Settings(Settings settings)
    {
        this._settings = settings;
        return this;
    }

    public FileService Build()
    {
        var fileServiceMoc = new Mock<FileService>(this._settings);
        fileServiceMoc.Setup(x => x.FileExists(It.IsAny<string>())).Returns(this._fileExists);
        fileServiceMoc.Setup(x => x.TryReadAllTextAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(this._tryReadAllTextAsync));
        return fileServiceMoc.Object;
    }
}