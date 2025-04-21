using Avalonia.Media;

namespace Hermes.Features.Controls.Primitives;

public interface IMaterial
{
    public ExperimentalAcrylicMaterial Material { get; set; }
    public bool MaterialIsVisible { get; set; }
}