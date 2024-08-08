using System.IO;
using System.Security.Cryptography;
using System;

namespace Hermes.Common;

public class AesEncryptor
{
    private static readonly byte[] Key32Bytes = "23451230114568563199079768789082"u8.ToArray();
    private static readonly byte[] Iv16Bytes = "5013456162789234"u8.ToArray();
 
    public string Encrypt(string text)
    {
        using var aes = Aes.Create();
        aes.Key = Key32Bytes;
        aes.IV = Iv16Bytes;
        var cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, cryptoTransform, CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
        sw.Write(text);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipheredText)
    {
        using var aes = Aes.Create();
        aes.Key = Key32Bytes;
        aes.IV = Iv16Bytes;
        var cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new(Convert.FromBase64String(cipheredText));
        using CryptoStream cs = new(ms, cryptoTransform, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
}