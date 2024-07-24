using Hermes.Builders;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using Moq;

namespace HermesTests.Services;

public class SfcServiceTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder =
        new(new UnitUnderTestBuilder(new FileService(), new Settings()));

    private readonly FileServiceMockBuilder _fileServiceMockBuilder = new();

    [Fact]
    public async Task Send_PassContent_ReturnsSfcResponsePass()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetPassContent()
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
        Assert.Equal(SfcErrorType.Timeout, (await sfcService.SendAsync(sfcResponse.UnitUnderTest!)).ErrorType);
    }

    private SfcService BuildSfcService(FileService fileService, Settings? settings = null)
    {
        var hermesContext = new HermesContext();
        var sfcResponseRepositoryMock = new Mock<SfcResponseRepository>(hermesContext);
        sfcResponseRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<SfcResponse>()))
            .Returns(Task.CompletedTask);

        var unitUnderTestRepositoryMock = new Mock<UnitUnderTestRepository>(hermesContext);
        unitUnderTestRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UnitUnderTest>()))
            .Returns(Task.CompletedTask);

        var sfcService = new SfcService(
            settings ?? new Settings(),
            fileService,
            unitUnderTestRepositoryMock.Object,
            sfcResponseRepositoryMock.Object
        );

        return sfcService;
    }
}