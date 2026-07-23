using Shared.Infrastructure.Data.Configurations.SecurityConfigurations;
using UserManagement.Domain.Enum;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        _ = builder.ToTable("modules", Schemas.UserManagement);

        // Relationship: Module → Application (Many-to-One)
        _ = builder.HasOne(m => m.Menu)
                   .WithMany()
                   .HasForeignKey(m => m.MenuId)
                   .OnDelete(DeleteBehavior.Restrict);



        var seedModules = new List<Module>
        {
            SeedModule.CompanyRole,  // ✅ Add this first
            SeedModule.AgentRole,
            SeedModule.UserRole,
            SeedModule.User,
        };

        _ = builder.HasData(seedModules);
    }
}
public class SeedModule
{
    private static DateTime LastUpdatedTime = DateTime.Parse("2026-07-21");

    #region UserManagement Modules

    /// <summary>
    /// CompanyRole Module - Linked to CompanyRole Menu
    /// </summary>
    public static Module CompanyRole = new(
        id: Guid.Parse("f7a8b9c0-d1e2-4f3a-8b9c-0d1e2f3a4b5c"),
        applicationId: SeedApplication.UserManagement.Id,
        name: "CompanyRole",
        description: "Company Role Management",
        code: ModuleEnum.CompanyRole,
        fromDate: LastUpdatedTime,
        menuId: SeedMenu.CompanyRole.Id
    );

    /// <summary>
    /// AgentRole Module - Linked to AgentRole Menu
    /// </summary>
    public static Module AgentRole = new(
        id: Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"),
        applicationId: SeedApplication.UserManagement.Id,
        name: "AgentRole",
        description: "Collection Center Role Management",
        code: ModuleEnum.AgentRole,
        fromDate: LastUpdatedTime,
        menuId: SeedMenu.AgentRole.Id
    );

    /// <summary>
    /// UserRole Module - Linked to UserRole Menu
    /// </summary>
    public static Module UserRole = new(
        id: Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"),
        applicationId: SeedApplication.UserManagement.Id,
        name: "UserRole",
        description: "User Role Management",
        code: ModuleEnum.UserRole,
        fromDate: LastUpdatedTime,
        menuId: SeedMenu.UserRole.Id
    );

    /// <summary>
    /// User Module - Linked to User Menu
    /// </summary>
    public static Module User = new(
        id: Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"),
        applicationId: SeedApplication.UserManagement.Id,
        name: "User",
        description: "User Management",
        code: ModuleEnum.User,
        fromDate: LastUpdatedTime,
        menuId: SeedMenu.User.Id
    );

    #endregion

    public static List<Module> GetAll() => new()
    {
        CompanyRole,
        AgentRole,
        UserRole,
        User,
    };
}
