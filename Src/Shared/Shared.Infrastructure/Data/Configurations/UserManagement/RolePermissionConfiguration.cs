namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RoleModulePermission>
{
    public void Configure(EntityTypeBuilder<RoleModulePermission> builder)
    {
        builder.ToTable("RolePermissions", Schemas.UserManagement);
        builder.HasKey(rp => new { rp.RoleId, rp.ModulePermissionId });
        builder.HasOne(rp => rp.Role).WithMany().HasForeignKey(rp => rp.RoleId);
        builder.HasOne(rp => rp.ModulePermission).WithMany().HasForeignKey(rp => rp.ModulePermissionId);
    }
}
