using Avalonia;
using Avalonia.Controls;
using Hermes.Types;
using Hermes.Common;

namespace Hermes.Features.Controls;

public partial class LEDControl : UserControl
{
    public static readonly StyledProperty<LEDState> StateProperty = AvaloniaProperty.Register<LEDControl, LEDState>(nameof(State),
        defaultValue: LEDState.Disconnect);

    public LEDState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    public LEDControl()
    {
        InitializeComponent();
    }
}