using System;
//using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Hermes.Models;
using Hermes.Repositories;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using System.Diagnostics;

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
    private readonly IServiceProvider _serviceProvider;

    public SlideBoardViewModel(
        SlideRepository slideRepository,
        Device device
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
        Console.WriteLine("SlideBoxes", SlideBoxes);
        // 订阅 Device.SlideBoxInPlace 的变化
        //this._serviceProvider = serviceProvider;
        //var device = this._serviceProvider.GetRequiredService<Device>();
        this._device = device;

        // 初始化时同步一次状态
        for (int i = 0; i < Math.Min(device.SlideBoxInPlace.Length, SlideBoxes.Count); i++)
        {
            SlideBoxes[i].BoxInPlace = device.SlideBoxInPlace[i];
        }
        Messenger.Register<HeartBeatMessage>(this, this.Receive);
    }

    public void Receive(object? recipient, HeartBeatMessage message)
    {
        for (int i = 0; i < Math.Min(this._device.SlideBoxInPlace.Length, SlideBoxes.Count); i++)
        {
            SlideBoxes[i].BoxInPlace = this._device.SlideBoxInPlace[i];
        }
        Debug.WriteLine("HeartBeatMessage更新完成");
    }

    protected override void SetupReactiveExtensions()
    {

    }

    public void Dispose()
    {
        Messenger.UnregisterAll(this);
    }
}

