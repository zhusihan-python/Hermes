using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;

namespace HermesIntegrationTests.Services;

public class FileServicesTests
{
    private readonly Settings _settings;
    private readonly FileService _sut;

    public FileServicesTests()
    {
        _settings = new Settings();
        _sut = new FileService(_settings);
        _sut.DeleteFolderIfExists(_settings.InputPath);
        _sut.DeleteFolderIfExists(_settings.BackupPath);
    }

    [Fact]
    public async Task TryReadAllTextAsync_FileExists_ReturnsContent()
    {
        const string content = "content";
        var fullPath = Path.Combine(_settings.InputPath, "test.txt");
        await _sut.WriteAllTextAsync(fullPath, content);
        Assert.Equal(content, await _sut.TryReadAllTextAsync(fullPath));
    }

    [Fact]
    public async Task TryReadAllTextAsync_NotFileExists_ReturnsEmptyString()
    {
        var fullPath = Path.Combine(_settings.InputPath, "doesNotExists.txt");
        Assert.Equal(string.Empty, await _sut.TryReadAllTextAsync(fullPath));
    }

    [Fact]
    public async Task WriteAllTextAsync_FileExists_WritesContentInFile()
    {
        const string content = "content";
        var fullPath = Path.Combine(_settings.InputPath, "test.txt");
        await _sut.WriteAllTextAsync(fullPath, content);
        Assert.True(_sut.FileExists(fullPath));
    }

    [Fact]
    public async Task MoveToBackupAsync_FileExists_MovesToBackup()
    {
        const string content = "content";
        var inputFullPath = Path.Combine(_settings.InputPath, "test.txt");
        await _sut.WriteAllTextAsync(inputFullPath, content);
        var backupFullPath = await _sut.MoveToBackupAsync(inputFullPath);
        Assert.False(_sut.FileExists(inputFullPath));
        Assert.True(_sut.FileExists(backupFullPath));
        Assert.Equal(content, await _sut.TryReadAllTextAsync(backupFullPath));
    }

    [Fact]
    public async Task CopyFromBackupToInputAsync_FileExists_CopyToInput()
    {
        const string content = "content";
        var inputFullPath = Path.Combine(_settings.InputPath, "test.txt");
        await _sut.WriteAllTextAsync(inputFullPath, content);
        var backupFullPath = await _sut.MoveToBackupAsync(inputFullPath);
        await _sut.CopyFromBackupToInputAsync(backupFullPath);
        Assert.True(_sut.FileExists(inputFullPath));
        Assert.Equal(content, await _sut.TryReadAllTextAsync(inputFullPath));
    }

    [Fact]
    public async Task DeleteFileIfExists_FileExists_DeletesFile()
    {
        const string content = "content";
        var inputFullPath = Path.Combine(_settings.InputPath, "test.txt");
        await _sut.WriteAllTextAsync(inputFullPath, content);
        Assert.True(_sut.FileExists(inputFullPath));
        await _sut.DeleteFileIfExists(inputFullPath);
        Assert.False(_sut.FileExists(inputFullPath));
    }
}