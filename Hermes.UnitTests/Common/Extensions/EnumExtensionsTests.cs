using HarfBuzzSharp;
using Hermes.Common.Extensions;
using Hermes.Types;

namespace HermesTests.Common.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void GetDescription_ValidEnum_ReturnsDescription()
    {
        Assert.Equal("En", LanguageType.En.GetDescription());
    }

    [Fact]
    public void GetEnumValues_ValidEnum_ReturnsAllValuesAsStringList()
    {
        var values = EnumExtensions.GetEnumValues<LanguageType>();
        Assert.NotEmpty(values);
        foreach (var languageType in Enum.GetValues(typeof(LanguageType)).Cast<LanguageType>())
        {
            Assert.Contains(languageType.ToString(), values);
        }
    }
}