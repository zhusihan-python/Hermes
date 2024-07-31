using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Moq;

namespace HermesTests.Common.Validators;

public class ConsecutiveDefectsValidatorTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public ConsecutiveDefectsValidatorTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public async void ValidateAsync_ConsecutiveDefects_ReturnStop()
    {
        var defect = new Defect();
        var defectRepositoryMock = GetDefectRepositoryMock(defect);
        var sfcResponse = _sfcResponseBuilder.Build();

        var validator = new ConsecutiveDefectsValidator(defectRepositoryMock);
        Assert.False((await validator.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async void ValidateAsync_NotConsecutiveDefects_ReturnStopNull()
    {
        var defectRepositoryMock = GetDefectRepositoryMock(Defect.Null);
        var sfcResponse = _sfcResponseBuilder.Build();

        var validator = new ConsecutiveDefectsValidator(defectRepositoryMock);
        Assert.True((await validator.ValidateAsync(sfcResponse)).IsNull);
    }

    private IDefectRepository GetDefectRepositoryMock(Defect? defect = null)
    {
        var defectRepositoryMock = new Mock<IDefectRepository>();
        defectRepositoryMock
            .Setup(x => x.GetConsecutiveSameDefects(It.IsAny<int>()))
            .Returns(Task.FromResult(defect ?? Defect.Null));
        return defectRepositoryMock.Object;
    }
}