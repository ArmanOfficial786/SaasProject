namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserConfiguration : IEntityTypeConfiguration<User>
{

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UserId"); // EF default Guid generation on add

        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Contact).HasMaxLength(256);
        builder.Property(u => u.PasswordHash).HasMaxLength(256);
        builder.Property(u => u.CompanyId).IsRequired();

        builder.HasOne(u => u.EntryBy)
            .WithMany()
            .HasForeignKey("EntryByUserId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.CompanyId, u.NormalizedEmail }).IsUnique();

        builder.Navigation(u => u.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserPermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserStatuses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}


