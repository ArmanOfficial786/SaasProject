namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        _ = builder.ToTable("login_logs", Schemas.UserManagement);
    }
}
