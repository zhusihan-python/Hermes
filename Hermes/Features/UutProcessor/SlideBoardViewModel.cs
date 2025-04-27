using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using R3;
using Microsoft.FSharp.Core;
using Hermes.Models;
using Hermes.Repositories;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using System.Diagnostics;
using Hermes.Types;
using Hermes.Communication.SerialPort;
using System.Collections.Generic;
using Hermes.Common.SlideSortCSharp;
using SlideSort;
using System.Net.Sockets;
using System.Buffers.Binary;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardViewModel : ViewModelBase
{
    private ObservableCollection<SlideBoxViewModel> _slideBoxes;
    public ObservableCollection<SlideBoxViewModel> SlideBoxes
    {
        get { return _slideBoxes; }
        set { SetProperty(ref _slideBoxes, value); }
    }
    private Queue<List<int>> TaskQueue;
    private Queue<List<int>> StartedTaskQueue;
    //private Queue<List<int>> InplaceTaskQueue;
    private Queue<List<int>> StartedInplaceTaskQueue;
    private bool canCheckInPlace;
    private int _rowCount = 5;
    public int RowCount
    {
        get => _rowCount;
    }
    private int _columnCount = 15;
    public int ColumnCount
    {
        get => _columnCount;
    }
    public Device _device;
    private readonly MessageSender _sender;
    private readonly SlideRepository _slideRepository;
    private static readonly char[] RowLabels = { 'A', 'B', 'C', 'D', 'E' };

    public SlideBoardViewModel(
        SlideRepository slideRepository,
        Device device,
        MessageSender sender
        )
    {
        this.TaskQueue = new Queue<List<int>>();
        this.StartedTaskQueue = new Queue<List<int>>();
        //this.InplaceTaskQueue = new Queue<List<int>>();
        this.StartedInplaceTaskQueue = new Queue<List<int>>();
        this.canCheckInPlace = false;
        SlideBoxes = new ObservableCollection<SlideBoxViewModel>();

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                SlideBoxes.Add(new SlideBoxViewModel(slideRepository) { RowIndex = i, ColumnIndex = j });
            }
        }
        //Console.WriteLine("SlideBoxes", SlideBoxes);
        this._device = device;
        this._sender = sender;
        this._slideRepository = slideRepository;

        // 初始化时同步一次状态
        for (int i = 0; i < Math.Min(device.SlideBoxInPlace.Length, SlideBoxes.Count); i++)
        {
            SlideBoxes[i].BoxInPlace = device.SlideBoxInPlace[i];
        }
        Messenger.Register<HeartBeatMessage>(this, this.Receive);
        Messenger.Register<SealSlideMessage>(this, this.StartSealSlide);
        Messenger.Register<SortSlideMessage>(this, this.StartSortSlide);
        Messenger.Register<SlideInfoMessage>(this, this.UpdateSlideInfo);
    }

    public void EnqueueTask(List<int> task)
    {
        TaskQueue.Enqueue(task);
    }

    public List<int> DequeueTask()
    {
        return TaskQueue.Count > 0 ? TaskQueue.Dequeue() : new List<int>();
    }

    public void EnqueueStartedTask(List<int> task)
    {
        StartedTaskQueue.Enqueue(task);
    }

    public List<int> DequeueStartedTask()
    {
        return StartedTaskQueue.Count > 0 ? StartedTaskQueue.Dequeue() : new List<int>();
    }

    //public void EnqueueInplaceTask(List<int> task)
    //{
    //    InplaceTaskQueue.Enqueue(task);
    //}

    //public List<int> DequeueInplaceTask()
    //{
    //    return InplaceTaskQueue.Count > 0 ? InplaceTaskQueue.Dequeue() : new List<int>();
    //}

    public void EnqueueStartedInplaceTask(List <int> task)
    {
        StartedInplaceTaskQueue.Enqueue(task);
    }

    public List<int> DequeueStartedInplaceTask()
    {
        return StartedInplaceTaskQueue.Count > 0 ? StartedInplaceTaskQueue.Dequeue() : new List<int>();
    }

    public List<int> PeekNextTask()
    {
        return TaskQueue.Count > 0 ? TaskQueue.Peek() : new List<int>();
    }

    public List<int> PeekStartedTask()
    {
        return StartedTaskQueue.Count > 0 ? StartedTaskQueue.Peek() : new List<int>();
    }

    //public List<int> PeekInplaceTask()
    //{
    //    return InplaceTaskQueue.Count > 0 ? InplaceTaskQueue.Peek() : new List<int>();

    //}

    public List<int> PeekStartedInplaceTask()
    {
        return StartedInplaceTaskQueue.Count > 0 ? StartedInplaceTaskQueue.Peek() : new List<int>();
    }

    public void Receive(object? recipient, HeartBeatMessage message)
    {
        if (this._device.SlideBoxInPlace.Length != SlideBoxes.Count)
        {
            return;
        }

        for (int i = 0; i < SlideBoxes.Count; i++)
        {
            var slideBoxViewModel = SlideBoxes[i];
            slideBoxViewModel.BoxInPlace = this._device.SlideBoxInPlace[i];
            slideBoxViewModel.ActionType = this._device.SlideBoxActions[i];
            slideBoxViewModel.IsBusy = this._device.SlideBoxActions[i].IsBusy();
            if (this._device.SlideBoxActions[i].IsSealSuccess())
            {
                string boxLocation = $"{RowLabels[i / 15]}-{i % 15 + 1}";
                ShowSuccessToast($"{boxLocation}封片成功", "提示");
            }
            else if (this._device.SlideBoxActions[i].IsSealFailed())
            {
                string boxLocation = $"{RowLabels[i / 15]}-{i % 15 + 1}";
                ShowWarningToast($"{boxLocation}封片失败", "警告");
            }

            var itemList = slideBoxViewModel.ItemList;

            for (int j = 0;j < itemList.Count; j++)
            {
                var item = itemList[j];
                item.State = this._device.SlideInPlace[i * itemList.Count + j].ToSlideState();
            }
        }
        CheckScanStarted(this._device.SlideBoxActions);
        var scanFinished = CheckScanTaskFinish(this._device.SlideBoxActions);
        if (scanFinished)
        {
            Debug.WriteLine("ScanTaskFinish is true");
            Observable.Timer(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    // 在 Timer 到期后执行此代码
                    this.canCheckInPlace = true;
                });
            //var dices = DequeueStartedTask();
            //foreach (int dice in dices)
            //{
            //    Debug.WriteLine($"cur Index {dice}");
            //    var slideBoxViewModel = SlideBoxes[dice];
            //    var slideList = slideBoxViewModel.ItemList;
            //    foreach(SlideModel model in slideList)
            //    {
            //        var slide = model.Slide;
            //        if (slide != null)
            //        {
            //            Debug.WriteLine($"Slide ID: {slide.SlideId}, PatientName: {slide.PatientName}");
            //        }
            //    }
            //}
        }
        //CheckInplaceStarted(this._device.SlideBoxActions);
        var inplaceCheckFinished = CheckInplaceFinish(this._device.SlideBoxActions);
        if (scanFinished && inplaceCheckFinished)
        {
            Debug.WriteLine("InplaceFinish is true");
            // 获取待理片的玻片信息
            var dices = DequeueStartedTask();
            var slides = new List<Tuple<SlideSorter.Slide, int>>();
            foreach (int dice in dices)
            {
                Debug.WriteLine($"cur Index {dice}");
                var slideBoxViewModel = SlideBoxes[dice];
                var slideList = slideBoxViewModel.ItemList;
                var boxIndex = slideBoxViewModel.RowIndex * this.ColumnCount + slideBoxViewModel.ColumnIndex;
                for (int k=0; k < slideList.Count;k++)
                {
                    var model = slideList[k];
                    var slide = model.Slide;
                    if (slide != null && !object.ReferenceEquals(slide, Slide.Null))
                    {
                        slides.Add(Tuple.Create(SlideManager.ToFSharpSlide(slide), boxIndex * 20 + k));
                        Debug.WriteLine($"Add Slide ID: {slide.SlideId}, PatientName: {slide.PatientName}");
                    }
                }
            }
            // 排序，获取位置信息
            var availablePositions = SlideManager.GenerateStatusArray(this._device.SlideBoxInPlace, this._device.SlideInPlace);
            var result = SlideManager.SortSlides(slides.ToArray(), SortKey.PathologyId, availablePositions);

            if (result.IsSuccess)
            {
                Debug.WriteLine("排序成功，移动序列：");

                if (FSharpOption<Tuple<int, int>[]>.get_IsSome(result.Moves))
                {
                    foreach (var (from, to) in result.Moves.Value)
                    {
                        Debug.WriteLine($"从位置 {from} 移动到位置 {to}");
                    }
                    var sortBatch = new SortWriteBatch(result.Moves.Value);
                    var batchMessages = sortBatch.GenerateSortBatchMessages();
                    Debug.WriteLine($"batchMessages length {batchMessages.Length}");
                    _ = Task.Run(async() =>
                    {
                        var totalSteps = new byte[2];
                        BinaryPrimitives.WriteInt16BigEndian(totalSteps, (Int16)batchMessages.Length);
                        var totalStepsPacket = new FlowActionWrite()
                                                    .WithOperationType(0x02)
                                                    .WithMasterAddress<FlowActionWrite>(0xF2)
                                                    .WithSlaveAddress<FlowActionWrite>(0x13)
                                                    .WithActionSequence(totalSteps)
                                                    .WithActionType(0x00)
                                                    .WithPickCount(0x00)
                                                    .WithSrcDstLocations(Enumerable.Repeat((byte)0x00, 20).ToArray())
                                                    .GenData();
                        await this._sender.EnqueueMessage(totalStepsPacket);
                        foreach (var packet in batchMessages)
                        {
                            await Task.Delay(100);
                            await this._sender.EnqueueMessage(packet);
                        }
                        await Task.Delay(100);
                        var executePacket = new SystemStatusWrite().
                                                    WithOperationType(0x05).
                                                    WithMasterAddress<SystemStatusWrite>(0xF2).
                                                    WithSlaveAddress<SystemStatusWrite>(0x13).
                                                    WithBoxTags(new byte[75]);
                        await this._sender.EnqueueMessage(executePacket);
                    });
                }
                else
                {
                    Debug.WriteLine("没有可用的移动数据。");
                }
            }
            else
            {
                Debug.WriteLine($"排序失败：{result.ErrorMessage.Value}");
            }
        }
        Debug.WriteLine("HeartBeatMessage更新完成");
    }

    public void StartSealSlide(object? recipient, SealSlideMessage message)
    {
        var boxTags = new byte[SlideBoxes.Count];
        var sealBoxCounts = 0;
        for (int i = 0; i < SlideBoxes.Count; i++)
        {
            var viewModel = SlideBoxes[i];
            if (viewModel.IsSelected)
            {
                boxTags[i] = 0x01;
                sealBoxCounts ++;
                Debug.WriteLine($"StartSealSlide row {viewModel.RowIndex} col {viewModel.ColumnIndex} selected {viewModel.IsSelected}");
            }
        }
        if (sealBoxCounts > 0)
        {
            var packet = new SystemStatusWrite().
                            WithOperationType(0x04).
                            WithMasterAddress<SystemStatusWrite>(0xF2).
                            WithSlaveAddress<SystemStatusWrite>(0x13).
                            WithBoxTags(boxTags);
            _ = Task.Run(() => this._sender.EnqueueMessage(packet));
            // remove selected  tag
            for (int j = 0;j < SlideBoxes.Count; j++)
            {
                var viewModel = SlideBoxes[j];
                if (boxTags[j] == 0x01)
                {
                    viewModel.IsSelected = false;
                }
            }
        }
    }

    public void StartSortSlide(object? recipient, SortSlideMessage message)
    {
        this.canCheckInPlace = false;
        var orderKey = 0;
        if (message != null)
        {
            orderKey = message.Value != -1 ? message.Value : 0;
        }
        var boxTags = new byte[SlideBoxes.Count];
        var unSelectedInplaceBoxes = new byte[SlideBoxes.Count];
        var sortBoxCounts = 0;
        for (int i = 0; i < SlideBoxes.Count; i++)
        {
            var viewModel = SlideBoxes[i];
            if (viewModel.IsSelected)
            {
                boxTags[i] = 0x01;
                sortBoxCounts ++;
                Debug.WriteLine($"StartSortSlide row {viewModel.RowIndex} col {viewModel.ColumnIndex} selected {viewModel.IsSelected}");
            }
            else if (viewModel.BoxInPlace)
            {
                unSelectedInplaceBoxes[i] = 0x01;
            }
        }
        if (sortBoxCounts > 0)
        {
            var packet = new SystemStatusWrite().
                            WithOperationType(0x03).
                            WithMasterAddress<SystemStatusWrite>(0xF2).
                            WithSlaveAddress<SystemStatusWrite>(0x13).
                            WithBoxTags(boxTags);
            _ = Task.Run(() => this._sender.EnqueueMessage(packet));
            // remove selected  tag
            var selectedBoxes = new List<int>();
            for (int j = 0; j < SlideBoxes.Count; j++)
            {
                var viewModel = SlideBoxes[j];
                if (boxTags[j] == 0x01)
                {
                    viewModel.IsSelected = false;
                    selectedBoxes.Add(j);
                }
            }
            EnqueueTask(selectedBoxes);
            if (unSelectedInplaceBoxes.Any())
            {
                var inplacePacket = new SystemStatusWrite().
                                        WithOperationType(0x02).
                                        WithMasterAddress<SystemStatusWrite>(0xF2).
                                        WithSlaveAddress<SystemStatusWrite>(0x13).
                                        WithBoxTags(unSelectedInplaceBoxes);
                _ = Task.Run(async() =>
                {
                    await Task.Delay(100);
                    await this._sender.EnqueueMessage(inplacePacket);
                });
                var indicesOfUnSelectedBoxes = Enumerable.Range(0, unSelectedInplaceBoxes.Length)
                                                         .Where(i => unSelectedInplaceBoxes[i] == 0x01)
                                                         .ToList();
                EnqueueStartedInplaceTask(indicesOfUnSelectedBoxes);
            }
        }
    }

    private void CheckScanStarted(SlideBoxActionType[] actionTypes)
    {
        var indices = PeekNextTask();
        if (indices.Count > 0)
        {
            if (indices.Any(i => i >= 0 && i < actionTypes.Length &&
                                   actionTypes[i] == SlideBoxActionType.ScanningCode))
            {
                EnqueueStartedTask(DequeueTask());
            }
        }
    }

    private bool CheckScanTaskFinish(SlideBoxActionType[] actionTypes)
    {
        var indices = PeekStartedTask();
        if (indices.Any())
        {
            return indices.All(i => i >= 0 && i < actionTypes.Length &&
                                   (actionTypes[i] == SlideBoxActionType.ScanCodeSuccess ||
                                    actionTypes[i] == SlideBoxActionType.ScanCodeFailed));
        }
        return false;
    }

    //private void CheckInplaceStarted(SlideBoxActionType[] actionTypes)
    //{
    //    var indices = PeekInplaceTask();
    //    if (indices.Count > 0)
    //    {
    //        if (indices.Any(i => i >= 0 && i < actionTypes.Length &&
    //                               actionTypes[i] == SlideBoxActionType.ScanningSlide))
    //        {
    //            EnqueueStartedInplaceTask(DequeueTask());
    //        }
    //    }
    //}

    private bool CheckInplaceFinish(SlideBoxActionType[] actionTypes)
    {
        if (canCheckInPlace)
        {
            var indices = PeekStartedInplaceTask();
            if (indices.Any())
            {
                return indices.All(i => i >= 0 && i < actionTypes.Length &&
                                       (actionTypes[i] == SlideBoxActionType.ScanSlideSuccess ||
                                        actionTypes[i] == SlideBoxActionType.ScanSlideFailed));
            }
        }

        return false;
    }

    private async void UpdateSlideInfo(object? recipient, SlideInfoMessage message)
    {
        if (message != null)
        {
            var (slideSeq, originBarcode) = message.Value;
            if (originBarcode.Length > 0)
            {
                var barcode = string.Concat(originBarcode.Select(b => (char)b));
                Debug.WriteLine($"barcode {barcode}");
                var slide = await this._slideRepository.FindById(barcode);
                var boxIndex = (slideSeq - 1) / 20;
                var slideIndex = (slideSeq - 1) % 20;
                var boxModel = this.SlideBoxes[boxIndex];
                boxModel.UpdateSlide(slideIndex, slide);
            }
            Debug.WriteLine("UpdateSlideInfo");
        }
    }

    protected override void SetupReactiveExtensions()
    {

    }

    public void Dispose()
    {
        Messenger.UnregisterAll(this);
    }
}

