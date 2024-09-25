using Hermes.Builders;
using Hermes.Common.Extensions;
using Hermes.Types;

namespace HermesTests.Builders;

public class UnderTestBuilderTests
{
    private UnitUnderTestBuilder _unitUnderTestBuilder;
    private const FileExtension FileExtension = Hermes.Types.FileExtension.Log;
    private const LogfileType LogfileType = Hermes.Types.LogfileType.TriDefault;

    public UnderTestBuilderTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder
            .InputFileExtension(FileExtension)
            .LogfileType(LogfileType);
    }

    [Fact]
    public void SetFilename_GivenFileName_FileNameIsSet()
    {
        string fileNameWithoutExtension = $"MyFileName";
        string fileName = $"{fileNameWithoutExtension}{FileExtension.GetDescription()}";
        var uut = this._unitUnderTestBuilder
            .FileNameWithoutExtension(fileNameWithoutExtension)
            .Build();
        Assert.Equal(fileName, uut.FileName);
    }

    [Fact]
    public void SetContent_ValidSerialNumber_SerialNumberIsParsed()
    {
        const string serialNumber = "1A62FMM00403397RM";
        var uut = this._unitUnderTestBuilder
            .SerialNumber(serialNumber)
            .Build();
        Assert.Equal(serialNumber, uut.SerialNumber);
    }

    [Fact]
    public void SetContent_NotValidSerialNumber_SerialNumberIsEmpty()
    {
        const string serialNumber = "@";
        var uut = this._unitUnderTestBuilder
            .SerialNumber(serialNumber)
            .Build();
        Assert.Equal(string.Empty, uut.SerialNumber);
    }

    [Fact]
    public void SetContent_WithDefects_DefectsParsedOk()
    {
        var uut = this._unitUnderTestBuilder
            .AddRandomDefect()
            .AddRandomDefect()
            .AddRandomDefect()
            .Build();
        Assert.Equal(this._unitUnderTestBuilder.Defects.Count, uut.Defects.Count);
    }

    [Fact]
    public void IsFail_PassLogfile_ReturnsFalse()
    {
        var uut = this._unitUnderTestBuilder
            .IsPass(true)
            .Build();
        Assert.False(uut.IsFail);
    }

    [Fact]
    public void IsFail_FailLogfile_ReturnsTrue()
    {
        var uut = this._unitUnderTestBuilder
            .IsPass(false)
            .Build();
        Assert.True(uut.IsFail);
    }
}