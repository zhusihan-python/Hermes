using Avalonia;
using Avalonia.Controls;
using Hermes.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Hermes.Features.Controls;

public partial class TaskBoard : UserControl
{
    public ObservableCollection<DistributedTask> TaskData
    {
        get => GetValue(TaskDataProperty);
        set => SetValue(TaskDataProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<DistributedTask>> TaskDataProperty =
        AvaloniaProperty.Register<TaskBoard, ObservableCollection<DistributedTask>>(nameof(TaskData), []);

    public TaskBoard()
    {
        InitializeComponent();
        TaskDataProperty.Changed.AddClassHandler<TaskBoard>(OnPropertyChanged);

        if (Design.IsDesignMode)
        {
            TaskData = new ObservableCollection<DistributedTask>
            {
                new DistributedTask("A-1", 0.75, "·âÆ¬+ÀíÆ¬"),
                new DistributedTask("A-15", 0.5, "·âÆ¬"),
                new DistributedTask("A-22", 0.25, "ÀíÆ¬")
            };
        }
    }

    private void Update()
    {
        //var margin = 10;
        //var width = SliderCanvas.Bounds.Width;
        //var height = SliderCanvas.Bounds.Height;
        //var knobWidth = SliderKnob.Bounds.Width;

        //if (!IsValidSize(width) || !IsValidSize(height) || !IsValidSize(knobWidth))
        //    return;

        //// Center the line
        //Canvas.SetLeft(SliderLine, margin);
        //SliderLine.Width = width - margin * 2;
    }

    private void UserControl_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        Update();
    }

    private void UserControl_SizeChanged(object? sender, global::Avalonia.Controls.SizeChangedEventArgs e)
    {
        Update();
    }

    private static void OnPropertyChanged(TaskBoard sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TaskDataProperty)
        {
            Debug.WriteLine("TaskDataProperty Changed");
            Debug.WriteLine($"sender TaskData Count {sender.TaskData.Count()}");
            sender.Update();
        }
    }

    private static bool IsValidSize(double size) => !(size == 0 || double.IsNaN(size));
}