using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Material.Icons;

namespace Hermes.Features.Logs;

public partial class LogsViewModel : PageBase
{
    public LogsViewModel()
        : base("UUT Processor", MaterialIconKind.Mace)
    {
    }
}

