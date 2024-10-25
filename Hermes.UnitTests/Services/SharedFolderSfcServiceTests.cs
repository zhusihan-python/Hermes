using Hermes.Builders;
using Hermes.Common.Reactive;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using Moq;
using R3;

namespace HermesTests.Services;

public class SharedFolderSfcServiceTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;
    private readonly FileServiceMockBuilder _fileServiceMockBuilder = new();

    public SharedFolderSfcServiceTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public async Task Send_PassContent_ReturnsSfcResponsePass()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetOkSfcResponse()
            .Build();
        var fileServiceMock = this._fileServiceMockBuilder
            .FileExists(true)
            .TryReadAllTextAsync(sfcResponse.Content)
            .Build();
        var sut = BuildSut(fileServiceMock);
        Assert.False((await sut.SendAsync(UnitUnderTest.Null)).IsFail);
    }

    [Fact]
    public async Task Send_Timeout_ReturnsSfcResponseTimeout()
    {
        var fileServiceMock = this._fileServiceMockBuilder
            .FileExists(false)
            .Build();
        var settings = new Settings()
        {
            SfcTimeoutSeconds = 0
        };
        var sut = BuildSut(fileServiceMock, settings);
        Assert.Equal(SfcResponseType.Timeout, (await sut.SendAsync(UnitUnderTest.Null)).Type);
    }

    private SharedFolderSfcService BuildSut(FileService fileService, Settings? settings = null)
    {
        var hermesContext = new HermesLocalContext();
        var sfcResponseRepositoryMock = new Mock<SfcResponseRepository>(hermesContext);
        sfcResponseRepositoryMock
            .Setup(x => x.AddAndSaveAsync(It.IsAny<SfcResponse>()))
            .Returns(Task.CompletedTask);
        var unitUnderTestRepositoryMock = new Mock<UnitUnderTestRepository>(hermesContext);
        unitUnderTestRepositoryMock
            .Setup(x => x.AddAndSaveAsync(It.IsAny<UnitUnderTest>()))
            .Returns(Task.CompletedTask);

        var folderWatcherServiceMock =
            new Mock<FolderWatcherService>(new FileService(settings ?? new Settings()), new FileSystemWatcherRx());
        folderWatcherServiceMock.Setup(x => x.TextDocumentCreated)
            .Returns(Observable.Never<TextDocument>());

        var sfcService = new SharedFolderSfcService(
            fileService,
            folderWatcherServiceMock.Object,
            settings ?? new Settings()
        );

        return sfcService;
    }
}