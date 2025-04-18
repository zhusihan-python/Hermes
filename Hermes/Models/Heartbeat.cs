using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Models;

public class Heartbeat
{
    public static readonly Heartbeat Null = new HeartbeatNull();

    [Key] public int Id { get; init; }

    public byte DevReset { get; init; }
    public byte SealMotRst { get; init; }
    public byte SortMotRst { get; init; }
    public byte SealMotFlow { get; init; }
    public byte SortMotFlow { get; init; }
    public byte SysActStat { get; init; }
    public ushort ScanTgt { get; init; }
    public ushort ActPackSeq { get; init; }
    public ushort ActPackRem { get; init; }
    public byte MotBrd1Stat { get; init; }
    public byte MotBrd2Stat { get; init; }
    public byte EnvBrdStat { get; init; }
    public float GasPress { get; init; }
    public float Suck1Press { get; init; }
    public float Suck2Press { get; init; }
    public byte BakeStat { get; init; }
    public float BakeTgtTemp { get; init; }
    public float BakeCurrTemp { get; init; }
    public uint BakeTgtTime { get; init; }
    public uint BakeRemTime { get; init; }
    public byte WasteBoxIn { get; init; }
    public byte CoverBoxIn { get; init; }
    public ushort CoverRemCnt { get; init; }
    [MaxLength(10)] public byte[] SlideBoxIn { get; init; } = new byte[10];
    [MaxLength(188)] public byte[] SlideInInfo { get; init; } = new byte[188];
    [MaxLength(75)] public byte[] SlideActStat { get; init; } = new byte[75];
    public float SensTemp { get; init; }
    public float SensHumi { get; init; }
    public byte SealLiq { get; init; }
    public byte XyleneLiq { get; init; }
    public byte CleanLiq { get; init; }
    public DateTime Ts { get; init; } = DateTime.Now;

    [NotMapped] public bool IsNull => this == Null;

    public Heartbeat()
    {
    }

    public Heartbeat(
        byte devReset,
        byte sealMotRst,
        byte sortMotRst,
        byte sealMotFlow,
        byte sortMotFlow,
        byte sysActStat,
        ushort scanTgt,
        ushort actPackSeq,
        ushort actPackRem,
        byte motBrd1Stat,
        byte motBrd2Stat,
        byte envBrdStat,
        float gasPress,
        float suck1Press,
        float suck2Press,
        byte bakeStat,
        float bakeTgtTemp,
        float bakeCurrTemp,
        uint bakeTgtTime,
        uint bakeRemTime,
        byte wasteBoxIn,
        byte coverBoxIn,
        ushort coverRemCnt,
        byte[] slideBoxIn,
        byte[] slideInInfo,
        byte[] slideActStat,
        float sensTemp,
        float sensHumi,
        byte sealLiq,
        byte xyleneLiq,
        byte cleanLiq)
    {
        DevReset = devReset;
        SealMotRst = sealMotRst;
        SortMotRst = sortMotRst;
        SealMotFlow = sealMotFlow;
        SortMotFlow = sortMotFlow;
        SysActStat = sysActStat;
        ScanTgt = scanTgt;
        ActPackSeq = actPackSeq;
        ActPackRem = actPackRem;
        MotBrd1Stat = motBrd1Stat;
        MotBrd2Stat = motBrd2Stat;
        EnvBrdStat = envBrdStat;
        GasPress = gasPress;
        Suck1Press = suck1Press;
        Suck2Press = suck2Press;
        BakeStat = bakeStat;
        BakeTgtTemp = bakeTgtTemp;
        BakeCurrTemp = bakeCurrTemp;
        BakeTgtTime = bakeTgtTime;
        BakeRemTime = bakeRemTime;
        WasteBoxIn = wasteBoxIn;
        CoverBoxIn = coverBoxIn;
        CoverRemCnt = coverRemCnt;
        SlideBoxIn = slideBoxIn;
        SlideInInfo = slideInInfo;
        SlideActStat = slideActStat;
        SensTemp = sensTemp;
        SensHumi = sensHumi;
        SealLiq = sealLiq;
        XyleneLiq = xyleneLiq;
        CleanLiq = cleanLiq;
    }
}

public class HeartbeatNull() : Heartbeat(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0f, 0f, 0f, 0, 0f, 0f, 0, 0, 0, 0, 0, new byte[10], new byte[188], new byte[75], 0f, 0f, 0, 0, 0)
{
}
