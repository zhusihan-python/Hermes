namespace Hermes.Cipher.UnitTests;

public class TokenGeneratorTests
{
    private readonly DateOnly _firstFullWeek2024 = new(2024, 1, 1);

    [Fact]
    public void GetWeekNumber_FirstFullWeekFirstDayReturns1()
    {
        var token = TokenGenerator.GetWeekNumber(_firstFullWeek2024);
        Assert.Equal(1, token);
    }

    [Fact]
    public void GetWeekNumber_FirstFullWeekLastDayReturns1()
    {
        var token = TokenGenerator.GetWeekNumber(_firstFullWeek2024.AddDays(6));
        Assert.Equal(1, token);
    }

    [Fact]
    public void GetWeekNumber_LastFullWeekLastDayReturns52()
    {
        var token = TokenGenerator.GetWeekNumber(_firstFullWeek2024.AddDays(-1));
        Assert.Equal(52, token);
    }

    [Fact]
    public void Generate_ValidDataReturns_Token()
    {
        var token = new TokenGenerator().Generate(
            1,
            1,
            _firstFullWeek2024);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void DecodeId_ValidDataReturns_Token()
    {
        var id = 1;
        var departmentId = 1;
        var token = new TokenGenerator().Generate(id, departmentId, _firstFullWeek2024);
        var decodedId = new TokenGenerator().DecodeId(token, departmentId, _firstFullWeek2024);
        Assert.Equal(id, decodedId);
    }

    [Fact]
    public void CipherId_LessThan4DigitId_Returns4CharacterToken()
    {
        Assert.Equal(4, TokenGenerator.CipherId(1, 1).Length);
    }

    [Fact]
    public void CipherId_HigherThan4DigitId_ReturnsIdDigitLenToken()
    {
        const int id = 123456;
        const int departmentId = 123;
        Assert.Equal(
            id.ToString().Length + departmentId.ToString().Length,
            TokenGenerator.CipherId(id, departmentId).Length);
    }
}