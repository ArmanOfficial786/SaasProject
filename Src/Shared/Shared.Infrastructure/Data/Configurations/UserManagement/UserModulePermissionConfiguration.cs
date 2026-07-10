namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserModulePermissionConfiguration : IEntityTypeConfiguration<UserModulePermission>
{
    public void Configure(EntityTypeBuilder<UserModulePermission> builder)
    {
        _ = builder.ToTable("user_module_permissions", Schemas.UserManagement);

        builder.HasKey(ump => ump.Id);

        builder.Property(ump => ump.UserId).IsRequired();
        builder.Property(ump => ump.ModulePermissionId).IsRequired();

        // Seed data (empty - typically populated through application)
        var seedUserModulePermissions = new List<UserModulePermission>();
        builder.HasData(seedUserModulePermissions);

        // Relationship to User
        builder.HasOne(ump => ump.User)
            .WithMany(u => u.UserModulePermissions)
            .HasForeignKey(ump => ump.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to ModulePermission
        builder.HasOne(ump => ump.ModulePermission)
            .WithMany()
            .HasForeignKey(ump => ump.ModulePermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on (UserId, ModulePermissionId)
        builder.HasIndex(ump => new { ump.UserId, ump.ModulePermissionId }).IsUnique();
    }
}
