namespace Shared.Infrastructure.DbContext.Configurations.UserManagement
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(p => p.PermissionId);
            builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Module).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Description).HasMaxLength(250);
            builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("IX_Permissions_Code");
        }
    }
}
