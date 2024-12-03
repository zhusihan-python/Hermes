using Hermes.Language;
using Material.Icons;

namespace Hermes.Features.Logs;

public partial class LogsViewModel(UnitUnderTestLogViewModel underTestLogViewModel, 
    SystemLogTabViewModel systemLogTabViewModel, SystemAlarmTabViewModel systemAlarmTabViewModel) : PageBase(
    "历史",
    MaterialIconKind.History,
    96)
{
    public UnitUnderTestLogViewModel UnitUnderTestLogViewModel { get; set; } = underTestLogViewModel;
    public SystemLogTabViewModel SystemLogTabViewModel { get; set; } = systemLogTabViewModel;
    public SystemAlarmTabViewModel SystemAlarmTabViewModel { get; set; } = systemAlarmTabViewModel;
}