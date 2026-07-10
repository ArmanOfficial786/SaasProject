namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RoleModulePermissionConfiguration : IEntityTypeConfiguration<RoleModulePermission>
{
    public void Configure(EntityTypeBuilder<RoleModulePermission> builder)
    {
        _ = builder.ToTable("role_module_permissions", Schemas.UserManagement);
        builder.HasKey(rmp => new { rmp.RoleId, rmp.ModulePermissionId });

        builder.Property(rmp => rmp.RoleId).IsRequired();
        builder.Property(rmp => rmp.ModulePermissionId).IsRequired();

        // Seed data - Assign module permissions to roles
        var seedRoleModulePermissions = new List<RoleModulePermission>
        {
            // Admin role gets all permissions
            SeedRoleModulePermission.AdminUserRead,
            SeedRoleModulePermission.AdminUserWrite,
            SeedRoleModulePermission.AdminUserUpdate,
            SeedRoleModulePermission.AdminUserDelete,
            SeedRoleModulePermission.AdminUserRoleRead,
            SeedRoleModulePermission.AdminUserRoleWrite,
            SeedRoleModulePermission.AdminUserRoleUpdate,
            SeedRoleModulePermission.AdminUserRoleDelete,
            SeedRoleModulePermission.AdminAgentRoleRead,
            SeedRoleModulePermission.AdminAgentRoleWrite,
            SeedRoleModulePermission.AdminAgentRoleUpdate,
            SeedRoleModulePermission.AdminAgentRoleDelete,

            // Manager role gets read/write permissions
            SeedRoleModulePermission.ManagerUserRead,
            SeedRoleModulePermission.ManagerUserWrite,
            SeedRoleModulePermission.ManagerUserRoleRead,
            SeedRoleModulePermission.ManagerUserRoleWrite,
            SeedRoleModulePermission.ManagerAgentRoleRead,
            SeedRoleModulePermission.ManagerAgentRoleWrite,

            // User role gets read-only permissions
            SeedRoleModulePermission.UserUserRead,
            SeedRoleModulePermission.UserUserRoleRead,
            SeedRoleModulePermission.UserAgentRoleRead,
        };
        builder.HasData(seedRoleModulePermissions);

        //relationships to role
        builder.HasOne(rmp => rmp.Role)
            .WithMany(r => r.RoleModulePermissions)
            .HasForeignKey(rmp => rmp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to ModulePermission
        builder.HasOne(rmp => rmp.ModulePermission)
               .WithMany(mp => mp.RoleModulePermissions)
               .HasForeignKey(rmp => rmp.ModulePermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SeedRoleModulePermission
{
    #region Admin Role Permissions - Full Access
    public static RoleModulePermission AdminUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"), // Admin role
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001") // User Read
    );

    public static RoleModulePermission AdminUserWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000002") // User Write
    );

    public static RoleModulePermission AdminUserUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000003") // User Update
    );

    public static RoleModulePermission AdminUserDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000004") // User Delete
    );

    public static RoleModulePermission AdminUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005") // UserRole Read
    );

    public static RoleModulePermission AdminUserRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000006") // UserRole Write
    );

    public static RoleModulePermission AdminUserRoleUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000007") // UserRole Update
    );

    public static RoleModulePermission AdminUserRoleDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000008") // UserRole Delete
    );

    public static RoleModulePermission AdminAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009") // AgentRole Read
    );

    public static RoleModulePermission AdminAgentRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000a") // AgentRole Write
    );

    public static RoleModulePermission AdminAgentRoleUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000b") // AgentRole Update
    );

    public static RoleModulePermission AdminAgentRoleDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000c") // AgentRole Delete
    );
    #endregion

    #region Manager Role Permissions - Read/Write
    public static RoleModulePermission ManagerUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"), // Manager role
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001") // User Read
    );

    public static RoleModulePermission ManagerUserWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000002") // User Write
    );

    public static RoleModulePermission ManagerUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005") // UserRole Read
    );

    public static RoleModulePermission ManagerUserRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000006") // UserRole Write
    );

    public static RoleModulePermission ManagerAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009") // AgentRole Read
    );

    public static RoleModulePermission ManagerAgentRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000a") // AgentRole Write
    );
    #endregion

    #region User Role Permissions - Read Only
    public static RoleModulePermission UserUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"), // User role
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001") // User Read
    );

    public static RoleModulePermission UserUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005") // UserRole Read
    );

    public static RoleModulePermission UserAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009") // AgentRole Read
    );
    #endregion
}
