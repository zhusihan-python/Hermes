using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia;
using Material.Styles.Themes.Base;
using Material.Styles.Themes;

namespace Hermes.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void MaterialIcon_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var materialTheme = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        materialTheme.BaseTheme =
            materialTheme.BaseTheme == BaseThemeMode.Light ? BaseThemeMode.Dark : BaseThemeMode.Light;
    }
}