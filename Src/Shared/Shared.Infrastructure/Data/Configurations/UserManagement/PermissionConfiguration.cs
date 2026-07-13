namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PermissionId")
            .ValueGeneratedNever();

        // Explicit CompanyId property for tenant isolation
        builder.Property(p => p.CompanyId).IsRequired();

        builder.Property(p => p.Code).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Module).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);

        //// Seed data
        //var seedPermissions = new List<Permission>
        //{
        //    SeedPermission.UserManagementView,
        //    SeedPermission.UserManagementCreate,
        //    SeedPermission.UserManagementEdit,
        //    SeedPermission.UserManagementDelete,
        //    SeedPermission.RoleManagementView,
        //    SeedPermission.RoleManagementCreate,
        //    SeedPermission.RoleManagementEdit,
        //    SeedPermission.RoleManagementDelete,
        //};
        //builder.HasData(seedPermissions);

        // Unique constraint on (CompanyId, Code)
        builder.HasIndex(p => new { p.CompanyId, p.Code }).IsUnique();
    }
}

public class SeedPermission
{
    // User Management Permissions
    public static Permission UserManagementView = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        CompanyId = 1,
        Code = "user.view",
        Module = "UserManagement",
        Description = "View users"
    };

    public static Permission UserManagementCreate = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
        CompanyId = 1,
        Code = "user.create",
        Module = "UserManagement",
        Description = "Create new users"
    };

    public static Permission UserManagementEdit = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
        CompanyId = 1,
        Code = "user.edit",
        Module = "UserManagement",
        Description = "Edit existing users"
    };

    public static Permission UserManagementDelete = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
        CompanyId = 1,
        Code = "user.delete",
        Module = "UserManagement",
        Description = "Delete users"
    };

    // Role Management Permissions
    public static Permission RoleManagementView = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
        CompanyId = 1,
        Code = "role.view",
        Module = "UserManagement",
        Description = "View roles"
    };

    public static Permission RoleManagementCreate = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
        CompanyId = 1,
        Code = "role.create",
        Module = "UserManagement",
        Description = "Create new roles"
    };

    public static Permission RoleManagementEdit = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000007"),
        CompanyId = 1,
        Code = "role.edit",
        Module = "UserManagement",
        Description = "Edit existing roles"
    };

    public static Permission RoleManagementDelete = new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000008"),
        CompanyId = 1,
        Code = "role.delete",
        Module = "UserManagement",
        Description = "Delete roles"
    };
}
