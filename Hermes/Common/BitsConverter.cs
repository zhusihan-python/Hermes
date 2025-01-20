using System;

namespace Hermes.Common;

public static class BitsConverter
{
    public static byte BitsToByte(bool[] bits)
    {
        if (bits == null || bits.Length == 0)
            return 0;

        byte result = 0;
        int length = Math.Min(bits.Length, 8);
        for (int i = 0; i < length; i++)
        {
            if (bits[i])
            {
                result |= (byte)(1 << i); // 将第 i 位设置为 1
            }
        }

        return result;
    }

    public static byte[] BitsToBytes(bool[] bits)
    {
        if (bits == null || bits.Length == 0)
            return Array.Empty<byte>();

        int byteCount = (bits.Length + 7) / 8; // 计算需要的字节数
        byte[] bytes = new byte[byteCount];

        for (int i = 0; i < byteCount; i++)
        {
            int startIndex = i * 8;
            int length = Math.Min(8, bits.Length - startIndex);

            bool[] subBits = new bool[8];
            Array.Copy(bits, startIndex, subBits, 0, length);

            bytes[i] = BitsToByte(subBits);
        }

        return bytes;
    }

    public static bool[] ByteToBit(byte value)
    {
        bool[] bits = new bool[8]; // 创建一个长度为 8 的 bool 数组

        for (int i = 0; i < 8; i++)
        {
            // 检查 value 的第 i 位是否为 1
            bits[i] = (value & (1 << i)) != 0;
        }

        return bits;
    }

    public static bool[] BytesToBits(byte[] bytes, int bitCount)
    {
        if (bytes == null || bytes.Length == 0 || bitCount <= 0)
            return Array.Empty<bool>();

        bool[] bits = new bool[bitCount];
        int index = 0;

        foreach (byte b in bytes)
        {
            for (int i = 0; i < 8; i++)
            {
                if (index >= bitCount)
                    return bits;

                bits[index] = (b & (1 << i)) != 0;
                index++;
            }
        }

        return bits;
    }
}
