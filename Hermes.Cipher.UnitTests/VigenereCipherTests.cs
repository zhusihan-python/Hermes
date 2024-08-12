namespace Hermes.Cipher.UnitTests;

public class VigenereCipherTests
{
    private const string Text = "Hermes";
    private const string Key = "202401";

    [Fact]
    public void Cipher_ValidTextAndKey_ReturnsCipherText()
    {
        var result = VigenereCipher.Cipher(Text, Key);
        Assert.NotEqual(Text, result);
        Assert.Equal(Text.Length, result.Length);
    }

    [Fact]
    public void Decode_ValidTextAndKey_ReturnsDecodedText()
    {
        var cipherText = VigenereCipher.Cipher(Text, Key);
        var result = VigenereCipher.Decode(cipherText, Key);
        Assert.Equal(Text.ToUpper(), result);
    }

    [Fact]
    public void Decode_ValidTextAndKeyWithSeed_ReturnsDecodedText()
    {
        const int seed = 5;
        var cipherText = VigenereCipher.Cipher(Text, Key, seed);
        var result = VigenereCipher.Decode(cipherText, Key, seed);
        Assert.Equal(Text.ToUpper(), result);
    }

    [Fact]
    public void Decode_ValidTextAndKeyWithDifferentSeed_ReturnsNotDecodedText()
    {
        const int seed = 5;
        var cipherText = VigenereCipher.Cipher(Text, Key, seed);
        var result = VigenereCipher.Decode(cipherText, Key, seed + 1);
        Assert.NotEqual(Text.ToUpper(), result);
    }
}