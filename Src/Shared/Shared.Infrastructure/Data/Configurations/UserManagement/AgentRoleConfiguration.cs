namespace Shared.Infrastructure.Data.Configurations.SecurityConfigurations;

public class AgentRoleConfiguration : IEntityTypeConfiguration<AgentRole>
{
    public void Configure(EntityTypeBuilder<AgentRole> builder)
    {
        _ = builder.ToTable("agent_roles", Schemas.UserManagement);

        // Seed data
        //var seedAgentRoles = new List<AgentRole>();
        //builder.HasData(seedAgentRoles);
    }
}
