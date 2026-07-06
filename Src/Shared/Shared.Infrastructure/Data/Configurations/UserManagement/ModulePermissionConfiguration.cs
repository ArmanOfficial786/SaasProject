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

        builder.Property(x => x.Permission)
         .HasConversion<string>()
         .HasMaxLength(50)
         .IsRequired();

        builder.HasOne(x => x.Module)
            .WithMany(m => m.ModulePermissions)
            .HasForeignKey(x => x.ModuleId);

        // Note: Seed data will be added through migration or by other means
        // as ModulePermission requires proper Permission entity references
    }
}
