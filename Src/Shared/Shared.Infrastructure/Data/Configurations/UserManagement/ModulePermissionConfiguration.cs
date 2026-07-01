using UserManagement.Domain.Enum;
using PermissionEnum = UserManagement.Domain.Enum.Permission;

namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class ModulePermissionConfiguration : IEntityTypeConfiguration<ModulePermission>
{
    public void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        _ = builder.ToTable("module_permissions", Schemas.UserManagement);
        _ = builder.HasKey
            (x => new
            {
                x.Id,
                x.Permission,
            });

        // Note: Seed data will be added through migration or by other means
        // as ModulePermission requires proper Permission entity references
    }
}
