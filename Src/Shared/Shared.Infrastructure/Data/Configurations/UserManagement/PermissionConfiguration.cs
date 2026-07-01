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

        // Unique constraint on (CompanyId, Code)
        builder.HasIndex(p => new { p.CompanyId, p.Code }).IsUnique();
    }
}
