using Hermes.Builders;
using Hermes.Services;

namespace HermesTests.Models.Parsers;

public class UnitUnderTestParserTests
{
    private UnitUnderTestBuilder Builder { get; } = new(new FileService());

    [Fact]
    public void SetFilename_GivenFileName_FileNameIsSet()
    {
        const string fileName = "MyName";
        var uut = this.Builder
            .FileName(fileName)
            .Build();
        Assert.Equal(fileName, uut.FileName);
    }

    [Fact]
    public void SetContent_ValidSerialNumber_SerialNumberIsParsed()
    {
        const string serialNumber = "1A62FMM00403397RM";
        var uut = this.Builder
            .SerialNumber(serialNumber)
            .Build();
        Assert.Equal(serialNumber, uut.SerialNumber);
    }

    [Fact]
    public void SetContent_NotValidSerialNumber_SerialNumberIsEmpty()
    {
        const string serialNumber = "@";
        var uut = this.Builder
            .SerialNumber(serialNumber)
            .Build();
        Assert.Equal(string.Empty, uut.SerialNumber);
    }

    [Fact]
    public void SetContent_WithDefects_DefectsParsedOk()
    {
        var uut = this.Builder
            .AddRandomDefect()
            .AddRandomDefect()
            .AddRandomDefect()
            .Build();
        Assert.Equal(this.Builder.Defects.Count, uut.Defects.Count);
    }

    [Fact]
    public void IsFail_PassLogfile_ReturnsFalse()
    {
        var uut = this.Builder
            .IsPass(true)
            .Build();
        Assert.False(uut.IsFail);
    }

    [Fact]
    public void IsFail_FailLogfile_ReturnsTrue()
    {
        var uut = this.Builder
            .IsPass(false)
            .Build();
        Assert.True(uut.IsFail);
    }

    [Fact]
    public void GetSfcFormatedContent_FailLogfile_ReturnsTrue()
    {
        var uut = this.Builder.Build();
        Assert.Equal(this.Builder.Content, uut.Content);
    }
}