using Hermes.Builders;
using Hermes.Common.Validators;
using Hermes.Models;
using Hermes.Repositories;
using Moq;

namespace HermesTests.Common.Validators;

public class SameDefectsWithin1HourValidatorTests
{
    private readonly UnitUnderTestBuilder _unitUnderTestBuilder;

    public SameDefectsWithin1HourValidatorTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public async Task ValidateAsync_WithAnyDefects_ReturnStop()
    {
        var defect = new Defect();
        var defectRepositoryMock = GetDefectRepositoryMock(defect);
        var sfcResponse = _unitUnderTestBuilder.Build();

        var sut = new SameDefectsWithin1HourValidator(defectRepositoryMock);
        Assert.False((await sut.ValidateAsync(sfcResponse)).IsNull);
    }

    [Fact]
    public async Task ValidateAsync_NotConsecutiveDefects_ReturnStopNull()
    {
        var defectRepositoryMock = GetDefectRepositoryMock(Defect.Null);
        var sfcResponse = _unitUnderTestBuilder.Build();

        var sut = new SameDefectsWithin1HourValidator(defectRepositoryMock);
        Assert.True((await sut.ValidateAsync(sfcResponse)).IsNull);
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
            .Setup(x => x.GetNotRestoredSameDefectsWithin1Hour(It.IsAny<int>()))
            .Returns(Task.FromResult(defects));
        return defectRepositoryMock.Object;
    }
}