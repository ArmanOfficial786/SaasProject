namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RoleModulePermissionConfiguration : IEntityTypeConfiguration<RoleModulePermission>
{
    public void Configure(EntityTypeBuilder<RoleModulePermission> builder)
    {
        _ = builder.ToTable("role_module_permissions", Schemas.UserManagement);
        builder.HasKey(rmp => new { rmp.RoleId, rmp.ModulePermissionId });

        builder.Property(rmp => rmp.RoleId).IsRequired();
        builder.Property(rmp => rmp.ModulePermissionId).IsRequired();



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
        //seed data for role module permissions
        var seedRoleModulePermissions = new List<RoleModulePermission>
        {
            // Admin role gets ALL permissions
            SeedRoleModulePermission.AdminCompanyRoleRead,
            SeedRoleModulePermission.AdminCompanyRoleWrite,
            SeedRoleModulePermission.AdminCompanyRoleUpdate,
            SeedRoleModulePermission.AdminCompanyRoleDelete,
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

            // Manager role gets read/write permissions (no delete)
            SeedRoleModulePermission.ManagerCompanyRoleRead,
            SeedRoleModulePermission.ManagerCompanyRoleWrite,
            SeedRoleModulePermission.ManagerUserRead,
            SeedRoleModulePermission.ManagerUserWrite,
            SeedRoleModulePermission.ManagerUserRoleRead,
            SeedRoleModulePermission.ManagerUserRoleWrite,
            SeedRoleModulePermission.ManagerAgentRoleRead,
            SeedRoleModulePermission.ManagerAgentRoleWrite,

            // User role gets read-only permissions
            SeedRoleModulePermission.UserCompanyRoleRead,
            SeedRoleModulePermission.UserUserRead,
            SeedRoleModulePermission.UserUserRoleRead,
            SeedRoleModulePermission.UserAgentRoleRead,
        };

        builder.HasData(seedRoleModulePermissions);
    }
}

public class SeedRoleModulePermission
{
    #region Admin Role Permissions - Full Access

    // CompanyRole Permissions
    public static RoleModulePermission AdminCompanyRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000001")
    );

    public static RoleModulePermission AdminCompanyRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000002")
    );

    public static RoleModulePermission AdminCompanyRoleUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000003")
    );

    public static RoleModulePermission AdminCompanyRoleDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000004")
    );

    // User Permissions
    public static RoleModulePermission AdminUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001")
    );

    public static RoleModulePermission AdminUserWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000002")
    );

    public static RoleModulePermission AdminUserUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000003")
    );

    public static RoleModulePermission AdminUserDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000004")
    );

    // UserRole Permissions
    public static RoleModulePermission AdminUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005")
    );

    public static RoleModulePermission AdminUserRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000006")
    );

    public static RoleModulePermission AdminUserRoleUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000007")
    );

    public static RoleModulePermission AdminUserRoleDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000008")
    );

    // AgentRole Permissions
    public static RoleModulePermission AdminAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009")
    );

    public static RoleModulePermission AdminAgentRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000a")
    );

    public static RoleModulePermission AdminAgentRoleUpdate = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000b")
    );

    public static RoleModulePermission AdminAgentRoleDelete = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000001"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000c")
    );
    #endregion

    #region Manager Role Permissions - Read/Write (No Delete)

    // CompanyRole Permissions - Read/Write
    public static RoleModulePermission ManagerCompanyRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000001")
    );

    public static RoleModulePermission ManagerCompanyRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000002")
    );

    // User Permissions - Read/Write
    public static RoleModulePermission ManagerUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001")
    );

    public static RoleModulePermission ManagerUserWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000002")
    );

    // UserRole Permissions - Read/Write
    public static RoleModulePermission ManagerUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005")
    );

    public static RoleModulePermission ManagerUserRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000006")
    );

    // AgentRole Permissions - Read/Write
    public static RoleModulePermission ManagerAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009")
    );

    public static RoleModulePermission ManagerAgentRoleWrite = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000002"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-00000000000a")
    );
    #endregion

    #region User Role Permissions - Read Only

    // CompanyRole Permissions - Read Only
    public static RoleModulePermission UserCompanyRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("60000000-0000-0000-0000-000000000001")
    );

    // User Permissions - Read Only
    public static RoleModulePermission UserUserRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000001")
    );

    // UserRole Permissions - Read Only
    public static RoleModulePermission UserUserRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000005")
    );

    // AgentRole Permissions - Read Only
    public static RoleModulePermission UserAgentRoleRead = new(
        roleId: Guid.Parse("10000000-0000-0000-0000-000000000003"),
        modulePermissionId: Guid.Parse("50000000-0000-0000-0000-000000000009")
    );
    #endregion
}
