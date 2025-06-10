using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common;
using Hermes.Common.Messages;
using Hermes.Communication.Protocol;
using Hermes.Models;
using Hermes.Types;
using Microsoft.Extensions.DependencyInjection;
using TouchSocket.Core;

namespace Hermes.Communication.SerialPort;

public class FrameParser : ObservableRecipient
{
    //private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private Device _device;

    public FrameParser(
        Device device,
        ILogger logger
    )
    {
        this._device = device;
        this._logger = logger;
    }
    public async Task Route(SvtRequestInfo request)
    {
        if (Enumerable.SequenceEqual(request.CMDID, Svt.HeartBeat) && request.FrameType == Svt.ReadSuccess)
        {
            await HeartBeatParse(request);
        }
        else if (Enumerable.SequenceEqual(request.CMDID, Svt.ScanTrigger) && request.FrameType == Svt.Write)
        {
            _logger.Info("Route ScanTriggerParse");
            await ScanTriggerParse(request);
        }
    }

    private async Task HeartBeatParse(SvtRequestInfo request)
    {
        var device = this._device;
        void InnerParse(byte[] data)
        {
            var span = new ReadOnlySpan<byte>(data);
            if (span.Length >= 400)
            {
                device!.DeviceResetState = (DeviceResetType)span[0];
                device.SealMotorResetState.Value = (SealMotorResetType)span[1];
                device.SortMotorResetState.Value = (SortMotorResetType)span[2];
                device.SealMotorFlowState.Value = (SealMotorFlowType)span[3];
                device.SortMotorFlowState.Value = (SortMotorFlowType)span[4];
                device.DeviceActionState.Value = (DeviceActionType)span[5];
                device.ScanTargetIndex.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(6, 2).ToArray(), 0);
                device.ActionPackNumber.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(8, 2).ToArray(), 0);
                device.ActionPackCount.Value = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(10, 2).ToArray(), 0);
                device.MotorBoardOneState.Value = span[12] != 0;
                device.MotorBoardTwoState.Value = span[13] != 0;
                device.EnvironBoardState.Value = span[14] != 0;
                device.GasTankPressure = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(15, 4).ToArray(), 0);
                device.SuckerOnePressure = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(19, 4).ToArray(), 0);
                device.SuckerTwoPressure = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(23, 4).ToArray(), 0);
                device.BakeState.Value = span[27] != 0;
                device.BakeTargetTemp.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(28, 4).ToArray(), 0);
                device.BakeRealTemp.Value = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(32, 4).ToArray(), 0);
                device.BakeTargetDuration.Value = TouchSocketBitConverter.BigEndian.ToUInt32(span.Slice(36, 4).ToArray(), 0);
                device.BakeLeftDuration.Value = TouchSocketBitConverter.BigEndian.ToUInt32(span.Slice(40, 4).ToArray(), 0);
                device.WasteBoxInPlace.Value = span[44] != 0;
                device.CoverBoxInPlace.Value = span[45] != 0;
                device.CoverBoxLeftCount = TouchSocketBitConverter.BigEndian.ToUInt16(span.Slice(46, 2).ToArray(), 0);
                var alarmCodesArray = span.Slice(48, 20).ToArray();
                var alarmCodes = Enumerable.Range(0, 10)
                         .Select(i => TouchSocketBitConverter.BigEndian.ToUInt16(alarmCodesArray.AsSpan().Slice(i * 2, 2).ToArray(), 0));
                device.AlarmCodes = alarmCodes.ToArray();
                device.SlideBoxInPlace = BitsConverter.BytesToBitsAsBitArray(span.Slice(68, 10).ToArray(), 75);
                device.SlideInPlace = BitsConverter.BytesToBitsAsBitArray(span.Slice(78, 188).ToArray(), 1500);

                for (int k = 0; k < 75; k++)
                {
                    device.SlideBoxActions[k] = (SlideBoxActionType)span[k+ 266];
                }
                device.BoardTemp = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(341, 4).ToArray(), 0);
                device.BoardHumidity = TouchSocketBitConverter.BigEndian.ToSingle(span.Slice(345, 4).ToArray(), 0);
                device.SealLiquidExist = span[349] != 0;
                device.XyleneExist = span[350] != 0;
                device.CleanLiquidExist = span[351] != 0;
            }
            Messenger.Send(new HeartBeatMessage(true));
        }
        InnerParse(request.Data);
        Debug.WriteLine("Finish HeartBeatParse");
        await Task.CompletedTask;
    }

    private async Task ScanTriggerParse(SvtRequestInfo request)
    {
        if (request.Data != null && request.Data.Length == 7)
        {
            var actionType = (TriggerActionType)request.Data[0];
            if (actionType == TriggerActionType.ScanCode)
            {
                var serviceProvider = ((App)Application.Current!).GetSingleServiceProvider();
                var sender = serviceProvider.GetService<MessageSender>();
                var slideSeq = TouchSocketBitConverter.BigEndian.ToUInt16(request.Data.Skip(1).Take(2).ToArray(), 0);
                var scanRequest = new ScanStartRequest(slideSeq, request.FrameNo);
                _logger.Info($"ScanTriggerParse slideSeq {slideSeq}");
                if (slideSeq >= 1 && slideSeq <= 1500)
                {
                    // 触发扫码解码
                    var successResponse = new ScanTriggerWriteResponse(request.FrameNo).
                        WithMasterAddress<ScanTriggerWriteResponse>(0xF2).
                        WithSlaveAddress<ScanTriggerWriteResponse>(0x13).
                        TriggerSuccess();
                    await sender!.EnqueueMessageWithFrameNo(successResponse);
                    await sender!.SendScannerMessageAsync(scanRequest);
                }
                else
                {
                    var failResponse = new ScanTriggerWriteResponse(request.FrameNo).
                                    WithMasterAddress<ScanTriggerWriteResponse>(0xF2).
                                    WithSlaveAddress<ScanTriggerWriteResponse>(0x13).
                                    TriggerFail();
                    await sender!.EnqueueMessageWithFrameNo(failResponse);
                }
            }
        }
    }
}
