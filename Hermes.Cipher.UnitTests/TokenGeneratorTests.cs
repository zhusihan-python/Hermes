namespace Hermes.Cipher.UnitTests;

public class TokenGeneratorTests
{
    private readonly DateOnly _firstFullWeek2024 = new(2024, 1, 1);

    [Fact]
    public void Generate_ValidData_ReturnsToken()
    {
        var token = new TokenGenerator().Generate(
            1,
            1,
            _firstFullWeek2024);
        Assert.NotEmpty(token);
        Assert.Contains(".", token);
    }

    [Fact]
    public void Decode_ValidData_ReturnsTrue()
    {
        const int id = 1;
        const int departmentId = 1;
        var sut = new TokenGenerator();
        var token = sut.Generate(id, departmentId, _firstFullWeek2024);
        Assert.True(sut.TryDecode(token, departmentId, _firstFullWeek2024, out var decodedId));
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void Decode_DifferentWeek_ReturnsFalse()
    {
        const int id = 1;
        const int departmentId = 1;
        var sut = new TokenGenerator();
        var token = sut.Generate(id, departmentId, _firstFullWeek2024.AddDays(7));
        Assert.False(sut.TryDecode(token, departmentId, _firstFullWeek2024, out _));
    }

    [Fact]
    public void Decode_NotValidToken_ReturnsFalse()
    {
        Assert.False(new TokenGenerator().TryDecode("notValidToken", 1, _firstFullWeek2024, out var decodedId));
        Assert.Equal(0, decodedId);
    }

    [Fact]
    public void GetWeekNumber_FirstFullWeekFirstDay_Returns1()
    {
        var token = new TokenGenerator().GetWeekNumber(_firstFullWeek2024);
        Assert.Equal(1, token);
    }

    [Fact]
    public void GetWeekNumber_FirstFullWeekLastDay_Returns1()
    {
        var token = new TokenGenerator().GetWeekNumber(_firstFullWeek2024.AddDays(6));
        Assert.Equal(1, token);
    }

    [Fact]
    public void GetWeekNumber_LastFullWeekLastDay_Returns52()
    {
        var token = new TokenGenerator().GetWeekNumber(_firstFullWeek2024.AddDays(-1));
        Assert.Equal(52, token);
    }

    [Fact]
    public void IntToAlphabet_ValidInt_ReturnsAlphabetChar()
    {
        Assert.Equal("A", TokenGenerator.NumberToAlphabet("0"));
        Assert.Equal("J", TokenGenerator.NumberToAlphabet("9"));
        Assert.Equal("JJ", TokenGenerator.NumberToAlphabet("99"));
    }

    [Fact]
    public void AlphabetToInt_ValidString_ReturnsInt()
    {
        Assert.Equal("0", TokenGenerator.AlphabetToNumber("A"));
        Assert.Equal("9", TokenGenerator.AlphabetToNumber("J"));
        Assert.Equal("00", TokenGenerator.AlphabetToNumber("AA"));
    }
}