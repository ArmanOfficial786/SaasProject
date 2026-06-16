namespace Shared.Infrastructure.DbContext.Configurations.UserManagement
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.RoleId);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Description).HasMaxLength(500);
            builder.HasIndex(r => new { r.TenantId, r.Name }).IsUnique().HasDatabaseName("IX_Roles_TenantId_Name");

            // Configure relationships as optional to avoid issues with global query filters
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired(false);

            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .IsRequired(false);
        }
    }
}
