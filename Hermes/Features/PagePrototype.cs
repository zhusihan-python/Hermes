using Hermes.Features.About;
using Hermes.Features.Login;
using Hermes.Features.Logs;
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
            PermissionType.OpenUutProcessor),

        new(typeof(LoginViewModel)),

        new(typeof(LogsViewModel),
            PermissionType.FreeAccess),

        new(typeof(UserAdminViewModel),
            PermissionType.OpenUserAdmin),

        new(typeof(UutProcessorViewModel),
            PermissionType.FreeAccess),

        new(typeof(AdminToolsViewModel),
            PermissionType.FreeAccess)
    ];

    public IServiceProvider? Provider { get; set; }

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