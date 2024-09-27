using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public interface ISfcRepository
{
    Task<int> UpdatePackageTrackingLoadedAt(string pkgid, string loadedAt = "SYSTIMESTAMP");
    Task<int> UpdatePackageTrackingLine(string pkgid, string line);
    Task<Package> FindPackageTracking(string pkgid);
    Task<IEnumerable<Package>> FindAllPackagesTrackingByPkgid(string pkgid);

    Task<IEnumerable<Package>> FindAllPackagesTrackingByDate(string line, DateTime fromDate,
        DateTime toDate);

    Task<IEnumerable<UnitUnderTest>> FindAllUnitsUnderTest(string pkgid);
    Task<Package> FindPackage(string pkgid);
    Task<WorkOrder> FindWorkOrder(string workOrder);
    Task<IEnumerable<Package>> FindAllPackages(string workOrder);
    Task<int> AddPackageTrack(Package package);
    bool VerifyNextGroup(string serialNumber, string expectedGroup, out string nextGroup);
    Task<Package> FindNextCanUsePackage(string line);
    Task<int> ResetPackageTrackingLoadedAt(string packageId);
    Task<int> DeletePackageTracking(string pkgid);
    Task<User> FindUser(string userName, string password);
    Task<IEnumerable<User>> FindAllUsers(DepartmentType department, UserLevel userLevel);

    Task<IEnumerable<User>> FindUserById(string searchEmployeeId, DepartmentType department,
        UserLevel userLevel);

    Task<int> UpdateUser(User user);
    Task<int> AddUser(User user);
    Task<int> DeleteUser(User user);
}