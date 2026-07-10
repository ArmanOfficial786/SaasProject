namespace Shared.Infrastructure.Data.Configurations.SecurityConfigurations;

public class AgentUserConfiguration : IEntityTypeConfiguration<AgentUser>
{
    public void Configure(EntityTypeBuilder<AgentUser> builder)
    {
        _ = builder.ToTable("agent_users", Schemas.UserManagement);

        // Seed data
        //var seedAgentUsers = new List<AgentUser>();
        //builder.HasData(seedAgentUsers);
    }
}
