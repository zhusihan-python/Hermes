using Hermes.Cipher.Types;
using Hermes.Features.UutProcessor;
using Hermes.Types;
using System.Collections.Generic;
using System;

namespace Hermes.Models;

internal class PagePermissionResolver(
    Type pageType,
    PermissionType permissionType = PermissionType.FreeAccess,
    List<StationType>? hideFromStation = null)
{
    public Type PageType { get; } = pageType;

    public bool HasPermission(User user, Settings settings)
    {
        if (user.Department == DepartmentType.Admin)
        {
            return true;
        }

        if (settings.AutostartUutProcessor && PageType == typeof(UutProcessorViewModel))
        {
            return true;
        }

        var userHasPermission = permissionType == PermissionType.FreeAccess ||
                                user.HasPermission(permissionType);
        var stationIsAllowed = !hideFromStation?.Contains(settings.Station) ?? true;
        return userHasPermission && stationIsAllowed;
    }
}