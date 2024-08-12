using System.Text;

namespace Hermes.Cipher;

public class VigenereCipher
{
    public static string Cipher(string text, string key, int seed = 0)
    {
        text = text.ToUpper();
        key = key.ToUpper();
        var sb = new StringBuilder();
        var fullKey = GenerateKey(text, key);
        for (var i = 0; i < text.Length; i++)
        {
            var x = (text[i] + fullKey[i] + seed) % 26;
            sb.Append((char)(x + 'A'));
        }

        return sb.ToString();
    }

    public static string Decode(string cipherText, string key, int seed = 0)
    {
        cipherText = cipherText.ToUpper();
        key = key.ToUpper();
        var sb = new StringBuilder();
        var fullKey = GenerateKey(cipherText, key);
        for (var i = 0; i < cipherText.Length; i++)
        {
            var x = (cipherText[i] - fullKey[i] - seed + 26) % 26;
            sb.Append((char)(x + 'A'));
        }

        return sb.ToString();
    }

    private static string GenerateKey(string text, string key)
    {
        while (key.Length < text.Length)
        {
            key += key[key.Length % key.Length];
        }

        return key;
    }
}