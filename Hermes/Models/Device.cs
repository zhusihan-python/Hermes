using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Types;
using ObservableCollections;
using R3;

namespace Hermes.Models;

public class Device : ObservableObject
{
    // 整机复位状态
    public ReactiveProperty<DeviceResetType> DeviceResetState { get; set; } = new(DeviceResetType.NotRest);
    // 封片电机组复位状态
    public ReactiveProperty<SealMotorResetType> SealMotorResetState { get; set; } = new(SealMotorResetType.NotRest);
    // 理片电机组复位状态
    public ReactiveProperty<SortMotorResetType> SortMotorResetState { get; set; } = new(SortMotorResetType.NotRest);
    // 封片电机组流程状态
    public ReactiveProperty<SealMotorFlowType> SealMotorFlowState { get; set; } = new(SealMotorFlowType.Idle);
    // 理片电机组流程状态
    public ReactiveProperty<SortMotorFlowType> SortMotorFlowState { get; set; } = new(SortMotorFlowType.Idle);
    // 系统动作状态
    public ReactiveProperty<DeviceActionType> DeviceActionState { get; set; } = new(DeviceActionType.Idle);
    // 扫片或扫码目标
    public ReactiveProperty<ushort> ScanTargetIndex { get; set; } = new(0);
    // 当前动作包序号
    public ReactiveProperty<ushort> ActionPackNumber { get; set; } = new(0);
    // 动作包未执行总数(包含执行中)
    public ReactiveProperty<ushort> ActionPackCount { get; set; } = new(0);
    // 电机板1状态
    public ReactiveProperty<bool> MotorBoardOneState { get; set; } = new(false);
    // 电机板2状态
    public ReactiveProperty<bool> MotorBoardTwoState { get; set; } = new(false);
    // 环境板状态
    public ReactiveProperty<bool> EnvironBoardState { get; set; } = new(false);
    // 气罐气压当前值
    public ReactiveProperty<float> GasTankPressure { get; set; } = new(0.0f);
    // 吸盘1气压当前值
    public ReactiveProperty<float> SuckerOnePressure { get; set; } = new(0.0f);
    // 吸盘2气压当前值
    public ReactiveProperty<float> SuckerTwoPressure { get; set; } = new(0.0f);
    // 烘干工作状态
    public ReactiveProperty<bool> BakeState { get; set; } = new(false);
    // 烘干设定温度(℃)
    public ReactiveProperty<float> BakeTargetTemp { get; set; } = new(0.0f);
    // 烘干当前温度(℃)
    public ReactiveProperty<float> BakeRealTemp { get; set; } = new(0.0f);
    // 烘干设定时间(S)
    public ReactiveProperty<uint> BakeTargetDuration { get; set; } = new(0);
    // 烘干剩余时间(S)
    public ReactiveProperty<uint> BakeLeftDuration { get; set; } = new(0);
    // 废片盒在位信息
    public ReactiveProperty<bool> WasteBoxInPlace { get; set; } = new(false);
    public ReactiveProperty<bool> CoverBoxInPlace { get; set; } = new(false);
    public ReactiveProperty<ushort> CoverBoxLeftCount { get; set; } = new(0);
    public ObservableRingBuffer<ushort> AlarmCodes { get; set; } = new ObservableRingBuffer<ushort>(new ushort[10]);
    // 玻片盒1-75在位
    public ObservableList<bool> SlideBoxInPlace { get; set; } = new ObservableList<bool>(new bool[75]);
    // 玻片1-1500在位
    public ObservableList<bool> SlideInPlace { get; set; } = new ObservableList<bool>(new bool[1500]);
    public ObservableList<SlideBoxActionType> SlideBoxActions { get; set; } = new ObservableList<SlideBoxActionType>(new SlideBoxActionType[75]);
}
