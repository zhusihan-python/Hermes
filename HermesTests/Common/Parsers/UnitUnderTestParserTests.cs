using Hermes.Builders;
using Hermes.Models;
using Hermes.Services;

namespace HermesTests.Utils.Parsers;

public class UnitUnderTestParserTests
{
    private UnitUnderTestBuilder _unitUnderTestBuilder;

    public UnitUnderTestParserTests(UnitUnderTestBuilder unitUnderTestBuilder)
    {
        this._unitUnderTestBuilder = unitUnderTestBuilder;
    }

    [Fact]
    public void SetFilename_GivenFileName_FileNameIsSet()
    {
        const string fileExtension = ".log";
        const string fileName = $"MyFileName.{fileExtension}";
        var uut = this._unitUnderTestBuilder
            .InputFileExtension(fileExtension)
            .FileName(fileName)
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

    [Fact]
    public void GetSfcFormatedContent_FailLogfile_ReturnsTrue()
    {
        var uut = this._unitUnderTestBuilder.Build();
        Assert.Equal(this._unitUnderTestBuilder.Content, uut.Content);
    }
}