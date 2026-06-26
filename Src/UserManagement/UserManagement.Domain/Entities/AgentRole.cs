using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class AgentRole : AuditableEntity
{
    public Role Role { get; private set; }

    public AgentRole(Role role)
    {
        Role = role;
    }

#pragma warning disable CS8618
    private AgentRole() { }
}
