using Hermes.Language;
using Material.Icons;

namespace Hermes.Features.Logs;

public partial class LogsViewModel(UnitUnderTestLogViewModel underTestLogViewModel,
                                   SlideManageTabViewModel slideManageTabViewModel) : 
    PageBase(
        "历史",
        MaterialIconKind.History,
        96)
{
    public UnitUnderTestLogViewModel UnitUnderTestLogViewModel { get; set; } = underTestLogViewModel;
    public SlideManageTabViewModel SlideManageTabViewModel { get; set; } = slideManageTabViewModel;
}