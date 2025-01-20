using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Common;

public static class BitsConverter
{
    public static byte BitsToByte(bool[] bits)
    {
        if (bits == null || bits.Length == 0)
            return 0;

        byte result = 0;
        for (int i = 0; i < 8; i++)
        {
            if (bits[i])
            {
                result |= (byte)(1 << i); // 将第 i 位设置为 1
            }
        }

        return result;
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
}
