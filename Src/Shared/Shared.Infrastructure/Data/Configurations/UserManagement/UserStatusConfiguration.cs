namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        _ = builder.ToTable("user_statuses", Schemas.UserManagement);

        builder.HasKey(us => us.Id);

        builder.Property(us => us.FromDate).IsRequired();
        builder.Property(us => us.ToDate).IsRequired(false);
        builder.Property(us => us.Remarks).HasMaxLength(500);

        // Seed data - don't seed, let application manage user statuses
        // as they are created with each user
    }
}
