using Hermes.Cipher.Extensions;
using Hermes.Features.About;
using Hermes.Features.Bender;
using Hermes.Features.Login;
using Hermes.Features.Logs;
using Hermes.Features.PackageId;
using Hermes.Features.SfcSimulator;
using Hermes.Features.UserAdmin;
using Hermes.Features.UutProcessor;
using Hermes.Models;
using Hermes.Types;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System;
using Hermes.Features.AdminTools;

namespace Hermes.Features;

public class PagePrototype(Settings settings)
{
    private readonly List<PagePermissionResolver> _pagePermissionResolvers =
    [
        new(typeof(AboutViewModel)),

        new(typeof(BenderViewModel),
            PermissionType.OpenBender,
            Only([StationType.Labeling, StationType.LabelingMachine])),

        new(typeof(LoginViewModel)),

        new(typeof(LogsViewModel),
            PermissionType.OpenLogs,
            AllExcept([StationType.Labeling])),

        new(typeof(PackageIdViewModel),
            stationFilter: Only([StationType.Labeling, StationType.LabelingMachine])),

        new(typeof(SfcSimulatorViewModel),
            PermissionType.OpenSfcSimulator),

        new(typeof(UserAdminViewModel),
            PermissionType.OpenUserAdmin,
            Only([StationType.Labeling])),

        new(typeof(UutProcessorViewModel),
            PermissionType.OpenUutProcessor,
            AllExcept([StationType.Labeling])),

        new(typeof(AdminToolsViewModel),
            PermissionType.OpenAdminTools)
    ];

    public IServiceProvider? Provider { get; set; }

    private static List<StationType> Only(IEnumerable<StationType> stations) => stations.ToList();

    private static List<StationType> AllExcept(IEnumerable<StationType> exceptStations) => EnumExtensions
        .GetValues<StationType>()
        .Where(x => !exceptStations.Contains(x) && x != StationType.None)
        .ToList();

    public List<PageBase> GetPages(User user)
    {
        return _pagePermissionResolvers
            .Where(x => x.HasPermission(user, settings))
            .Select(x => Provider?.GetRequiredService(x.PageType) as PageBase)
            .Where(x => x != null)
            .Select(x => x!)
            .OrderBy(x => x.Index)
            .ToList();
    }
}