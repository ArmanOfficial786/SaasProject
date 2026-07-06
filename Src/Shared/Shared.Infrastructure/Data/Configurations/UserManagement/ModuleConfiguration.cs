using Shared.Infrastructure.Data.Configurations.SecurityConfigurations;
using UserManagement.Domain.Enum;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        _ = builder.ToTable("modules", Schemas.UserManagement);
        // Explicit key — don't rely purely on convention once there's any
        // chance of type ambiguity in this file
        builder.HasKey(m => m.Id);
        var seedModules = new List<Module>
        {
            SeedModule.AgentRole,
            SeedModule.UserRole,
            SeedModule.User,

        };

        _ = builder.HasData(seedModules);
    }
}

public class SeedModule
{
    private static DateTime LastUpdatedTime = DateTime.Parse("2024-06-06");
    public static Module AgentRole = new(Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"), SeedApplication.UserManagement.Id, "AgentRole", "Collection Center Role", ModuleEnum.AgentRole, LastUpdatedTime, SeedMenu.AgentRole.Id);
    public static Module UserRole = new(Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"), SeedApplication.UserManagement.Id, "UserRole", "User Role", ModuleEnum.UserRole, LastUpdatedTime, SeedMenu.UserRole.Id);
    public static Module User = new(Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"), SeedApplication.UserManagement.Id, "User", "User", ModuleEnum.User, LastUpdatedTime, SeedMenu.User.Id);

}
