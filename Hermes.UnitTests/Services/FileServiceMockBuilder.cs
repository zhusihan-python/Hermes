using Hermes.Models;
using Hermes.Services;
using Moq;

namespace HermesTests.Services;

public class FileServiceMockBuilder
{
    private bool _fileExists = true;
    private string _tryReadAllTextAsync = String.Empty;
    private GeneralSettings _generalSettings = new();

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
        var fileServiceMock = new Mock<FileService>(this._generalSettings);
        fileServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(this._fileExists);
        fileServiceMock.Setup(x => x.TryReadAllTextAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(this._tryReadAllTextAsync));
        return fileServiceMock.Object;
    }
}