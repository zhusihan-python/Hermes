using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using System;

namespace Hermes.Features.Controls;

public partial class StoreBox : UserControl
{
    public static readonly StyledProperty<double> PercentProperty =
        AvaloniaProperty.Register<StoreBox, double>(nameof(Percent), defaultValue: 100);

    public static readonly StyledProperty<IBrush> ColorProperty =
        AvaloniaProperty.Register<StoreBox, IBrush>(nameof(Color), Brushes.Cyan);

    public double Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public IBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private readonly UniformGrid _grid;

    public StoreBox()
    {
        var border = new Border
        {
            BorderBrush = Brush.Parse("#18FFFF"),
            BorderThickness = new Thickness(5),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(5),
            Child = _grid = new UniformGrid
            {
                Rows = 10,
                Columns = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            }
        };

        Content = border;
        this.Width = 120;
        this.Height = 200;
        this.Percent = 20;
        // Observe property changes and trigger updates automatically
        this.GetObservable(PercentProperty).Subscribe(_ => UpdateStoreBox());

        // Initialize rectangles
        UpdateStoreBox();
    }

    private void UpdateStoreBox()
    {
        _grid.Children.Clear();

        int noActiveCount = 10 - (int)Math.Round(Percent / 10); // Determine number of rectangles to show
        Color = Percent switch
        {
            >= 100 => Brushes.Green,
            >= 50 => Brushes.Cyan,
            _ => Brushes.Red
        };

        for (int i = 0; i < 10; i++)
        {
            var rect = new Rectangle
            {
                Fill = i < noActiveCount ? Brushes.Transparent : Color,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                Margin = new Thickness(2)
            };

            _grid.Children.Add(rect);
        }
    }
}