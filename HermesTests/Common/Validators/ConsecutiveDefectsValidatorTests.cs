using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Moq;

namespace HermesTests.Common.Validators;

public class ConsecutiveDefectsValidatorTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public ConsecutiveDefectsValidatorTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public async Task ValidateAsync_ConsecutiveDefects_ReturnStop()
    {
        var defect = new Defect();
        var defectRepositoryMock = GetDefectRepositoryMock(defect);
        var unitUnderTest = _unitUnderTestBuilder.Build();

        var validator = new ConsecutiveDefectsValidator(defectRepositoryMock);
        Assert.False((await validator.ValidateAsync(unitUnderTest)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_NotConsecutiveDefects_ReturnStopNull()
    {
        var defectRepositoryMock = GetDefectRepositoryMock(Defect.Null);
        var unitUnderTest = _unitUnderTestBuilder.Build();

        var validator = new ConsecutiveDefectsValidator(defectRepositoryMock);
        Assert.True((await validator.ValidateAsync(unitUnderTest)).IsNull);
    }

    private IDefectRepository GetDefectRepositoryMock(Defect? defect = null)
    {
        var defects = new List<Defect>();
        if (defect is { IsNull: false })
        {
            defects.Add(defect);
        }

        var defectRepositoryMock = new Mock<IDefectRepository>();
        defectRepositoryMock
            .Setup(x => x.GetNotRestoredConsecutiveSameDefects(It.IsAny<int>()))
            .Returns(Task.FromResult(defects));
        return defectRepositoryMock.Object;
    }
}