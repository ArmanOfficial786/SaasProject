using UserManagement.Domain.Enum;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class ModulePermissionConfiguration : IEntityTypeConfiguration<ModulePermission>
{
    public void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        _ = builder.ToTable("module_permissions", Schemas.UserManagement);
        _ = builder.HasKey(x => x.Id);

        builder.Property(x => x.ModuleId).IsRequired();

        builder.Property(x => x.Permission)
         .HasConversion<string>()
         .HasMaxLength(50)
         .IsRequired();

        builder.HasOne(x => x.Module)
            .WithMany(m => m.ModulePermissions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        //var seedModulePermissions = new List<ModulePermission>
        //{
        //    SeedModulePermission.UserModuleRead,
        //    SeedModulePermission.UserModuleWrite,
        //    SeedModulePermission.UserModuleUpdate,
        //    SeedModulePermission.UserModuleDelete,
        //    SeedModulePermission.UserRoleModuleRead,
        //    SeedModulePermission.UserRoleModuleWrite,
        //    SeedModulePermission.UserRoleModuleUpdate,
        //    SeedModulePermission.UserRoleModuleDelete,
        //    SeedModulePermission.AgentRoleModuleRead,
        //    SeedModulePermission.AgentRoleModuleWrite,
        //    SeedModulePermission.AgentRoleModuleUpdate,
        //    SeedModulePermission.AgentRoleModuleDelete,
        //};
        //builder.HasData(seedModulePermissions);

        // Unique constraint on (ModuleId, Permission)
        builder.HasIndex(x => new { x.ModuleId, x.Permission }).IsUnique();

        // Note: Seed data will be added through migration or by other means
        // as ModulePermission requires proper Permission entity references
    }
}

public class SeedModulePermission
{
    // User Module Permissions
    public static ModulePermission UserModuleRead = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000001"),
        moduleId: Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"), // User module
        permission: PermissionEnum.Read
    );

    public static ModulePermission UserModuleWrite = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000002"),
        moduleId: Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"),
        permission: PermissionEnum.Write
    );

    public static ModulePermission UserModuleUpdate = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000003"),
        moduleId: Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"),
        permission: PermissionEnum.Update
    );

    public static ModulePermission UserModuleDelete = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000004"),
        moduleId: Guid.Parse("65d5de5a-3b73-4e45-8775-1b3d6f144268"),
        permission: PermissionEnum.Delete
    );

    // UserRole Module Permissions
    public static ModulePermission UserRoleModuleRead = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000005"),
        moduleId: Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"), // UserRole module
        permission: PermissionEnum.Read
    );

    public static ModulePermission UserRoleModuleWrite = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000006"),
        moduleId: Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"),
        permission: PermissionEnum.Write
    );

    public static ModulePermission UserRoleModuleUpdate = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000007"),
        moduleId: Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"),
        permission: PermissionEnum.Update
    );

    public static ModulePermission UserRoleModuleDelete = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000008"),
        moduleId: Guid.Parse("ba51d83f-8c02-4fb5-922f-650b945b79b2"),
        permission: PermissionEnum.Delete
    );

    // AgentRole Module Permissions
    public static ModulePermission AgentRoleModuleRead = new(
        id: Guid.Parse("50000000-0000-0000-0000-000000000009"),
        moduleId: Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"), // AgentRole module
        permission: PermissionEnum.Read
    );

    public static ModulePermission AgentRoleModuleWrite = new(
        id: Guid.Parse("50000000-0000-0000-0000-00000000000a"),
        moduleId: Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"),
        permission: PermissionEnum.Write
    );

    public static ModulePermission AgentRoleModuleUpdate = new(
        id: Guid.Parse("50000000-0000-0000-0000-00000000000b"),
        moduleId: Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"),
        permission: PermissionEnum.Update
    );

    public static ModulePermission AgentRoleModuleDelete = new(
        id: Guid.Parse("50000000-0000-0000-0000-00000000000c"),
        moduleId: Guid.Parse("e3c916fb-608f-42b3-87db-1c46ae5b5148"),
        permission: PermissionEnum.Delete
    );
}
