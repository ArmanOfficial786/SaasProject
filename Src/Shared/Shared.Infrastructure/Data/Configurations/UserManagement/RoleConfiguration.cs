namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", Schemas.UserManagement);
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Desc).HasMaxLength(500);
        builder.HasIndex(r => new { r.CompanyId, r.Name }).IsUnique().HasDatabaseName("IX_Roles_CompanyId_Name");

        // Configure relationships
        builder.HasMany(r => r.UserRoles)
            .WithOne()
            .IsRequired(false);
    }
}
