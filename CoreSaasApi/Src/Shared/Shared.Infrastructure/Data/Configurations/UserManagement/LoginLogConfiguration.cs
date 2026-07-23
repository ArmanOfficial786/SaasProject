namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        _ = builder.ToTable("login_logs", Schemas.UserManagement);

        builder.HasKey(ll => ll.Id);

        builder.Property(ll => ll.IpAddress).HasMaxLength(45).IsRequired();
        builder.Property(ll => ll.MacAddress).HasMaxLength(50).IsRequired(false);
        builder.Property(ll => ll.ClientAgent).HasMaxLength(100).IsRequired();
        builder.Property(ll => ll.OS).HasMaxLength(100).IsRequired(false);
        builder.Property(ll => ll.LoginDate).IsRequired();

        // Relationship to User
        builder.HasOne(ll => ll.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
