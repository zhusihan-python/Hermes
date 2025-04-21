using System;
using System.Linq;
using System.Collections.ObjectModel;
//using Microsoft.Extensions.DependencyInjection;
using Hermes.Models;
using Hermes.Repositories;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using System.Diagnostics;
using Hermes.Types;
using Hermes.Communication.SerialPort;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardViewModel : ViewModelBase
{
    private ObservableCollection<SlideBoxViewModel> _slideBoxes;
    public ObservableCollection<SlideBoxViewModel> SlideBoxes
    {
        get { return _slideBoxes; }
        set { SetProperty(ref _slideBoxes, value); }
    }
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
            this._sender.EnqueueMessage(packet);
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
        var boxTags = new byte[SlideBoxes.Count];
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
        }
        if (sortBoxCounts > 0)
        {
            var packet = new SystemStatusWrite().
                            WithOperationType(0x03).
                            WithMasterAddress<SystemStatusWrite>(0xF2).
                            WithSlaveAddress<SystemStatusWrite>(0x13).
                            WithBoxTags(boxTags);
            this._sender.EnqueueMessage(packet);
            // remove selected  tag
            for (int j = 0; j < SlideBoxes.Count; j++)
            {
                var viewModel = SlideBoxes[j];
                if (boxTags[j] == 0x01)
                {
                    viewModel.IsSelected = false;
                }
            }
        }
    }

    private async void UpdateSlideInfo(object? recipient, SlideInfoMessage message)
    {
        if (message != null)
        {
            var (slideSeq, originBarcode) = message.Value;
            if (originBarcode.Length > 0)
            {
                var barcode = string.Concat(originBarcode.Select(b => (char)b));
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

