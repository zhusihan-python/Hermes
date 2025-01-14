﻿using Hermes.Communication.Protocol;
using System;
using System.IO;
using System.Linq;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class SvtRequestInfo : IRequestInfo, IRequestInfoBuilder
{
    private byte[] m_cMDID;
    private byte[] m_data;
    private byte[] m_cRC16;

    public int MaxLength => this.DataLength + 20;

    /// <summary>
    /// 报文头: 固定为2位 值为0x3C0x28 字符表示为"<("
    /// </summary>
    public byte[] Head { get; set; }

    /// <summary>
    /// 报文序号: 固定为2位 由主设备指定，从设备在回复命令时，帧序号需与接收到的命令中的保持一致
    /// </summary>
    public byte[] FrameNo { get; set; }

    /// <summary>
    /// 报文总长度
    /// </summary>
    public ushort PacketLength { get; set; }

    /// <summary>
    /// 地址组长度: 固定为1位
    /// </summary>
    public byte AddressLength = 2;

    /// <summary>
    /// 源设备地址
    /// </summary>
    public byte MasterAddress { get; set; }

    /// <summary>
    /// 目标设备地址
    /// </summary>
    public byte SlaveAddress { get; set; }

    /// <summary>
    /// 命令码: 固定2位
    /// </summary>
    public byte[] CMDID { get => this.m_cMDID; set => this.m_cMDID = value; }

    /// <summary>
    /// 帧类型: 固定1位 0x55:读 0xAA:读回应 0x66:写 0x88:写回应成功 0x99:写回应失败
    /// </summary>
    public byte FrameType { get; set; }

    /// <summary>
    /// 数据长度: 固定2位 数据段的长度
    /// </summary>
    public ushort DataLength { get; set; }

    /// <summary>
    /// 数据区: 长度为BodyLength的Value
    /// </summary>
    public byte[] Data { get => this.m_data; set => this.m_data = value; }

    /// <summary>
    /// 校验位：固定2位 校验内容为报文序号至数据区所有内容，不包括报文头和报文尾
    /// </summary>
    public byte[] CRC16 { get => this.m_cRC16; set => this.m_cRC16 = value; }

    /// <summary>
    /// 报文尾: 固定2位 值为0x290x3E 字符表示为")>"
    /// </summary>
    public byte Tail { get; set; }

    public byte[] DataFrame()
    {
        // 计算总长度
        ushort totalLength = (ushort)(12 + DataLength);
        this.PacketLength = (ushort)(totalLength + 6);

        // 使用 MemoryStream 和 BinaryWriter 连接数据
        using (var memoryStream = new MemoryStream(totalLength))
        using (var writer = new BinaryWriter(memoryStream))
        {
            // 写入 FrameNo（2 字节）
            writer.Write(FrameNo);

            // 写入 PacketLength（2 字节）
            writer.Write(PacketLength);

            // 写入 AddressLength（1 字节）
            writer.Write(AddressLength);

            // 写入 MasterAddress（1 字节）
            writer.Write(MasterAddress);

            // 写入 SlaveAddress（1 字节）
            writer.Write(SlaveAddress);

            // 写入 CMDID（2 字节）
            writer.Write(CMDID);

            // 写入 FrameType（1 字节）
            writer.Write(FrameType);

            // 写入 DataLength（2 字节）
            writer.Write(BitConverter.GetBytes(DataLength).Reverse().ToArray());

            // 写入 Data（可变长度）
            if (Data != null && Data.Length > 0)
            {
                writer.Write(Data);
            }

            // 返回最终的字节数组
            return memoryStream.ToArray();
        }
    }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var msgFrame = this.DataFrame();
        var crcAscii = Crc16.ComputeCrcArray(msgFrame, msgFrame.Length);

        byteBlock.Write(Svt.FullHead);
        byteBlock.Write(AddSymbol(msgFrame));
        byteBlock.Write(AddSymbol(crcAscii));
        byteBlock.Write(Svt.FullTail);
    }

    public static ReadOnlySpan<byte> AddSymbol(byte[] inputArray)
    {
        if (inputArray == null || inputArray.Length == 0)
        {
            // 如果数组为空或长度为0，直接返回原数组
            return inputArray;
        }
        int count = 0;
        byte[] packBuffer = new byte[inputArray.Length * 2];

        //如果协议帧包含关键字，添加隔断符
        for (int i = 0; i < inputArray.Length; i++)
        {
            packBuffer[count++] = inputArray[i];
            if (inputArray[i] == Svt.StartByte)
            {
                packBuffer[count++] = Svt.InsertStartByte;
            }
            if (inputArray[i] == Svt.EndByte)
            {
                packBuffer[count++] = Svt.InsertEndByte;
            }
        }
        return packBuffer.AsSpan(0, count);
    }
}