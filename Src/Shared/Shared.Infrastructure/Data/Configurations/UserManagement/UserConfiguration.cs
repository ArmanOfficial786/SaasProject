

namespace Shared.Infrastructure.DbContext.Configurations.UserManagement
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(250);
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.FullName).HasMaxLength(200);
            builder.Property(u => u.Password).IsRequired();

            builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique().HasDatabaseName("IX_Users_TenantId_Email");
            builder.HasIndex(u => new { u.TenantId, u.UserName }).IsUnique().HasDatabaseName("IX_Users_TenantId_UserName");

            // Configure relationship as optional to avoid issues with global query filters
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
