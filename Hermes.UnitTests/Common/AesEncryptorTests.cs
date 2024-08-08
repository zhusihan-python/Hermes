using Hermes.Common;

namespace HermesTests.Common;

public class AesEncryptorTests
{
    private const string Text = "test123@#";

    [Fact]
    public void Encrypt_ValidText_ReturnsEncryptedText()
    {
        var sut = new AesEncryptor();
        Assert.NotEqual(Text, sut.Encrypt(Text));
    }

    [Fact]
    public void EncryptAndDecrypt_ValidText_ReturnsSameText()
    {
        var sut = new AesEncryptor();
        var encryptedData = sut.Encrypt(Text);
        Assert.Equal(Text, sut.Decrypt(encryptedData));
    }
}