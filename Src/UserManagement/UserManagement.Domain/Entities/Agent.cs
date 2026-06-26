////| Agent            | Allowed Roles           |
//| ---------------- | ----------------------- |
//| Kathmandu Branch | Manager, Teller |
//| Pokhara Branch | Teller |
//| Head Office | Admin, Manager, Auditor |


using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class Agent : AuditableEntity
{
    public Guid CompanyId { get; private set; }
    [MaxLength(250)]
    public string? Name { get; private set; }
    [MinLength(9)]
    [MaxLength(9)]
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public bool IsParent { get; private set; }
    [MaxLength(50)]
    public string? ReferralCode { get; private set; }

    private readonly List<AgentUser> _agentUsers = [];
    public IReadOnlyCollection<AgentUser> AgentUsers => _agentUsers.AsReadOnly();
    private readonly List<AgentRole> _rolesForUser = [];
    public IReadOnlyCollection<AgentRole> RolesForUser => _rolesForUser.AsReadOnly();
}
