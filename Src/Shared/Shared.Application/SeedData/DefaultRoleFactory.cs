// File: Shared.Application/SeedData/DefaultRoleFactory.cs
using UserManagement.Domain.Entities;

namespace Shared.Application.SeedData;

/// <summary>
/// Builds the default Admin/Manager/User role set for a company.
/// Used by both DbInitializer and CreateCompanyCommandHandler.
/// No HasData/migration-time seeding is involved.
/// </summary>
public static class DefaultRoleFactory
{
    public static (Role Admin, Role Manager, Role User) CreateForCompany(int companyId)
    {
        var admin = new Role(
            companyId: companyId,
            name: "Admin",
            desc: "Administrator with full access")
        {
            NormalizedName = "ADMIN",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var manager = new Role(
            companyId: companyId,
            name: "Manager",
            desc: "Manager with operational access")
        {
            NormalizedName = "MANAGER",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var user = new Role(
            companyId: companyId,
            name: "User",
            desc: "Regular user with limited access")
        {
            NormalizedName = "USER",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        return (admin, manager, user);
    }
}
