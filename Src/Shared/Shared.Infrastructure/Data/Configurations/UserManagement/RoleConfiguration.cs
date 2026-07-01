namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("RoleId");
        // Explicit CompanyId property for tenant isolation
        builder.Property(r => r.CompanyId).IsRequired();
        builder.Property(r => r.Desc).HasMaxLength(500).IsRequired();

        builder.HasOne(r => r.EntryBy)
            .WithMany()
            .HasForeignKey("EntryByUserId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint per tenant
        builder.HasIndex(r => new { r.CompanyId, r.NormalizedName }).IsUnique();

        builder.Navigation(r => r.RoleModulePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
