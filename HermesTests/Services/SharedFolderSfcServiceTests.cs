using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using Moq;

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
            .SetOkContent()
            .Build();
        var fileServiceMock = this._fileServiceMockBuilder
            .FileExists(true)
            .TryReadAllTextAsync(sfcResponse.Content)
            .Build();
        var sfcService = BuildSfcService(fileServiceMock);
        Assert.False((await sfcService.SendAsync(sfcResponse.UnitUnderTest!)).IsFail);
    }

    [Fact]
    public async Task Send_Timeout_ReturnsSfcResponseTimeout()
    {
        var sfcResponse = this._sfcResponseBuilder.Build();
        var fileServiceMock = this._fileServiceMockBuilder
            .FileExists(false)
            .Build();
        var settings = new Settings()
        {
            SfcTimeoutSeconds = 0
        };
        var sfcService = BuildSfcService(fileServiceMock, settings);
        Assert.Equal(SfcResponseType.Timeout, (await sfcService.SendAsync(sfcResponse.UnitUnderTest!)).ResponseType);
    }

    private SharedFolderSfcService BuildSfcService(FileService fileService, Settings? settings = null)
    {
        var hermesContext = new HermesContext();
        var sfcResponseRepositoryMock = new Mock<SfcResponseRepository>(hermesContext);
        sfcResponseRepositoryMock
            .Setup(x => x.AddAndSaveAsync(It.IsAny<SfcResponse>()))
            .Returns(Task.CompletedTask);

        var unitUnderTestRepositoryMock = new Mock<UnitUnderTestRepository>(hermesContext);
        unitUnderTestRepositoryMock
            .Setup(x => x.AddAndSaveAsync(It.IsAny<UnitUnderTest>()))
            .Returns(Task.CompletedTask);

        var sfcService = new SharedFolderSfcService(
            settings ?? new Settings(),
            fileService,
            unitUnderTestRepositoryMock.Object,
            sfcResponseRepositoryMock.Object
        );

        return sfcService;
    }
}