namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", Schemas.UserManagement);
        builder.HasKey(ur => ur.Id);
        builder.HasOne(ur => ur.Role).WithMany().HasForeignKey("RoleId");
    }
}
