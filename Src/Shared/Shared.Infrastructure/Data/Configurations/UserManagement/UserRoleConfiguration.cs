namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", Schemas.UserManagement);
        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();

        // Seed data - Assign roles to users
        // Note: UserRole objects cannot be directly seed via HasData because role FK references must resolve
        // These will be manually inserted via DbInitializer or created through runtime seeding

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Field-access for base entity properties
        builder.Navigation(ur => ur.Role).UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
