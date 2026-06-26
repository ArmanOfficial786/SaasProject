namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", Schemas.UserManagement);
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(250);
        builder.Property(u => u.UserName).HasMaxLength(100);
        builder.Property(u => u.FullName).HasMaxLength(200);
        builder.Property(u => u.Contact).HasMaxLength(256);

        builder.HasIndex(u => new { u.CompanyId, u.Email }).IsUnique().HasDatabaseName("IX_Users_CompanyId_Email");
        builder.HasIndex(u => new { u.CompanyId, u.UserName }).IsUnique().HasDatabaseName("IX_Users_CompanyId_UserName");

        // Configure relationship
        builder.HasMany(u => u.UserRoles)
            .WithOne()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
