using System.Collections.Generic;

namespace Hermes.Types;

public enum AlarmLevel
{
    Warning,     // 警告
    Error,       // 故障
}

public struct AlarmCodeInfoStruct
{
    public int Code { get; set; }
    public string Meaning { get; set; }
    public AlarmLevel Level { get; set; }

    public AlarmCodeInfoStruct(int code, string meaning, AlarmLevel level)
    {
        Code = code;
        Meaning = meaning;
        Level = level;
    }

    public override string ToString()
    {
        return $"报警码: {Code}, 含义: {Meaning}, 级别: {Level}";
    }
}

public static class AlarmCodes
{
    public static readonly Dictionary<int, AlarmCodeInfoStruct> AlarmMap = new Dictionary<int, AlarmCodeInfoStruct>()
    {
        { 1, new AlarmCodeInfoStruct(10, "开机系统上电", AlarmLevel.Warning) },
        { 10, new AlarmCodeInfoStruct(10, "封片电机板系统上电", AlarmLevel.Warning) },
        { 11, new AlarmCodeInfoStruct(11, "电机(X轴)复位异常", AlarmLevel.Error) },
        { 12, new AlarmCodeInfoStruct(12, "电机(Y轴)复位异常", AlarmLevel.Error) },
        { 13, new AlarmCodeInfoStruct(13, "电机(Z轴)复位异常", AlarmLevel.Error) },
        { 14, new AlarmCodeInfoStruct(14, "电机(T轴)复位异常", AlarmLevel.Error) },
        { 15, new AlarmCodeInfoStruct(15, "电机(X轴)运动异常", AlarmLevel.Error) },
        { 16, new AlarmCodeInfoStruct(16, "电机(Y轴)运动异常", AlarmLevel.Error) },
        { 17, new AlarmCodeInfoStruct(17, "电机(Z轴)运动异常", AlarmLevel.Error) },
        { 18, new AlarmCodeInfoStruct(18, "电机(T轴)运动异常", AlarmLevel.Error) },
        { 50, new AlarmCodeInfoStruct(50, "理片电机板系统上电", AlarmLevel.Warning) },
        { 51, new AlarmCodeInfoStruct(51, "电机A复位异常", AlarmLevel.Error) },
        { 52, new AlarmCodeInfoStruct(52, "电机B复位异常", AlarmLevel.Error) },
        { 53, new AlarmCodeInfoStruct(53, "电机C复位异常", AlarmLevel.Error) },
        { 54, new AlarmCodeInfoStruct(54, "电机D复位异常", AlarmLevel.Error) },
        { 55, new AlarmCodeInfoStruct(55, "电机A运动异常", AlarmLevel.Error) },
        { 56, new AlarmCodeInfoStruct(56, "电机B运动异常", AlarmLevel.Error) },
        { 57, new AlarmCodeInfoStruct(57, "电机C运动异常", AlarmLevel.Error) },
        { 58, new AlarmCodeInfoStruct(58, "电机D运动异常", AlarmLevel.Error) },
        { 59, new AlarmCodeInfoStruct(59, "取片失败(气路压力异常)", AlarmLevel.Error) },
        { 60, new AlarmCodeInfoStruct(60, "取片失败(抓手已有其他序号玻片)", AlarmLevel.Error) },
        { 61, new AlarmCodeInfoStruct(61, "取片失败(抓手打开失败)", AlarmLevel.Error) },
        { 62, new AlarmCodeInfoStruct(62, "取片失败(抓手关闭失败)", AlarmLevel.Error) },
        { 63, new AlarmCodeInfoStruct(63, "取片失败(未检测到玻片)", AlarmLevel.Error) },
        { 64, new AlarmCodeInfoStruct(64, "取片失败(此序号无法取片)", AlarmLevel.Error) },
        { 65, new AlarmCodeInfoStruct(65, "取片失败(运动异常)", AlarmLevel.Error) },
        { 66, new AlarmCodeInfoStruct(66, "放片失败(未检测到玻片)", AlarmLevel.Error) },
        { 67, new AlarmCodeInfoStruct(67, "放片失败(气路压力异常)", AlarmLevel.Error) },
        { 68, new AlarmCodeInfoStruct(68, "放片失败(运动异常)", AlarmLevel.Error) },
        { 69, new AlarmCodeInfoStruct(69, "取片失败(抓手打开失败)", AlarmLevel.Error) },
        { 70, new AlarmCodeInfoStruct(70, "扫片失败(气路压力异常)", AlarmLevel.Error) },
        { 71, new AlarmCodeInfoStruct(71, "扫片失败(未指定扫描对象)", AlarmLevel.Error) },
        { 72, new AlarmCodeInfoStruct(72, "扫片失败(夹具存在玻片)", AlarmLevel.Error) },
        { 73, new AlarmCodeInfoStruct(73, "扫片失败(抓手打开失败)", AlarmLevel.Error) },
        { 74, new AlarmCodeInfoStruct(74, "扫码失败(气路压力异常)", AlarmLevel.Error) },
        { 75, new AlarmCodeInfoStruct(75, "扫码失败(未指定扫码对象)", AlarmLevel.Error) },
        { 76, new AlarmCodeInfoStruct(76, "扫码失败(夹具存在玻片)", AlarmLevel.Error) },
        { 77, new AlarmCodeInfoStruct(77, "扫码失败(抓手打开失败)", AlarmLevel.Error) },
        { 78, new AlarmCodeInfoStruct(78, "扫码失败(抓手关闭失败)", AlarmLevel.Error) },
        { 79, new AlarmCodeInfoStruct(79, "扫码失败(抓手旋转失败)", AlarmLevel.Error) },
        { 100, new AlarmCodeInfoStruct(100, "玻片墙控制板上电", AlarmLevel.Warning) },
        { 101, new AlarmCodeInfoStruct(101, "正压传感器异常", AlarmLevel.Error) },
        { 102, new AlarmCodeInfoStruct(102, "负压传感器1异常", AlarmLevel.Error) },
        { 103, new AlarmCodeInfoStruct(103, "负压传感器2异常", AlarmLevel.Error) },
        { 400, new AlarmCodeInfoStruct(400, "封片电机板通信异常", AlarmLevel.Error) },
        { 401, new AlarmCodeInfoStruct(401, "理片电机板通信异常", AlarmLevel.Error) },
        { 402, new AlarmCodeInfoStruct(402, "玻片墙主控板板通信异常", AlarmLevel.Error) },
        { 403, new AlarmCodeInfoStruct(403, "气罐升压超时(升压超过600s,压力未到)", AlarmLevel.Error) },
        { 404, new AlarmCodeInfoStruct(404, "气罐泄压超时(泄压大于30S，压力未降)", AlarmLevel.Error) },
        { 405, new AlarmCodeInfoStruct(405, "封片异常(没有盖片)", AlarmLevel.Error) },
        { 406, new AlarmCodeInfoStruct(406, "封片异常(封片剂缺液)", AlarmLevel.Error) },
        { 407, new AlarmCodeInfoStruct(407, "封片异常(气管压力异常)", AlarmLevel.Error) },
        { 408, new AlarmCodeInfoStruct(408, "封片异常(取片异常)", AlarmLevel.Error) },
        { 409, new AlarmCodeInfoStruct(409, "封片异常(放片异常)", AlarmLevel.Error) },
        { 410, new AlarmCodeInfoStruct(410, "封片异常(吸盘1压力异常)", AlarmLevel.Error) },
        { 411, new AlarmCodeInfoStruct(411, "封片异常(吸盘2压力异常)", AlarmLevel.Error) },
        { 412, new AlarmCodeInfoStruct(412, "封片异常(封片电机组运动异常)", AlarmLevel.Error) },
        { 413, new AlarmCodeInfoStruct(413, "封片异常(加液针未抬起)", AlarmLevel.Error) },
        { 414, new AlarmCodeInfoStruct(414, "理片异常(取片异常)", AlarmLevel.Error) },
        { 415, new AlarmCodeInfoStruct(415, "理片异常(放片异常)", AlarmLevel.Error) },
        { 416, new AlarmCodeInfoStruct(416, "理片异常(气管压力异常)", AlarmLevel.Error) },
        { 417, new AlarmCodeInfoStruct(417, "扫片异常(气管压力异常)", AlarmLevel.Error) },
        { 418, new AlarmCodeInfoStruct(418, "扫码异常(气管压力异常)", AlarmLevel.Error) },
        { 419, new AlarmCodeInfoStruct(419, "系统复位失败", AlarmLevel.Error) },
        { 420, new AlarmCodeInfoStruct(420, "扫片失败", AlarmLevel.Error) },
        { 421, new AlarmCodeInfoStruct(421, "扫码失败", AlarmLevel.Error) },
        { 422, new AlarmCodeInfoStruct(422, "封片失败", AlarmLevel.Error) },
        { 423, new AlarmCodeInfoStruct(423, "动作包执行失败", AlarmLevel.Error) },
    };
}