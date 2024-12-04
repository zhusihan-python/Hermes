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
        new(typeof(AboutViewModel),
            PermissionType.OpenSfcSimulator),

        new(typeof(BenderViewModel),
            PermissionType.OpenSfcSimulator),

        new(typeof(LoginViewModel)),

        new(typeof(LogsViewModel),
            PermissionType.FreeAccess),

        new(typeof(PackageIdViewModel),
            hideFromStation: AllExcept([StationType.Labeling, StationType.LabelingMachine])),

        new(typeof(SfcSimulatorViewModel),
            PermissionType.OpenSfcSimulator),

        new(typeof(UserAdminViewModel),
            PermissionType.OpenUserAdmin,
            Only([StationType.Labeling])),

        new(typeof(UutProcessorViewModel),
            PermissionType.FreeAccess),

        new(typeof(AdminToolsViewModel),
            PermissionType.FreeAccess)
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