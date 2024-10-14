using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.Logs;

public partial class LogsViewModel : PageBase
{
    public UnitUnderTestLogViewModel UnitUnderTestLogViewModel { get; set; }

    public LogsViewModel(UnitUnderTestLogViewModel underTestLogViewModel)
        : base("UUT Processor", MaterialIconKind.History, PermissionType.OpenLogs)
    {
        this.UnitUnderTestLogViewModel = underTestLogViewModel;
    }
}

