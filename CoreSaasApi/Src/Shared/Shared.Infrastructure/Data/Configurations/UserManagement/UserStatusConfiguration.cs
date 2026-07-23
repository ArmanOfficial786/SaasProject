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

        // ✅ Relationship: UserStatus → User
        builder.HasOne(us => us.User)
               .WithMany(u => u.UserStatuses)
               .HasForeignKey(us => us.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // ✅ Indexes for performance
        builder.HasIndex(us => us.UserId);
        builder.HasIndex(us => new { us.UserId, us.FromDate });

        // Seed data - don't seed, let application manage user statuses
        // as they are created with each user

        builder.HasData(
     new
     {
         Id = 1,
         UserId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
         FromDate = DateTime.UtcNow,
         ToDate = (DateTime?)null,
         Remarks = "Default owner user created"
     }
 );
    }
}
