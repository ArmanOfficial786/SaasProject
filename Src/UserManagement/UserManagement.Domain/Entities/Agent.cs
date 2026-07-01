////| Agent            | Allowed Roles           |
//| ---------------- | ----------------------- |
//| Kathmandu Branch | Manager, Teller |
//| Pokhara Branch | Teller |
//| Head Office | Admin, Manager, Auditor |


using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class Agent : AuditableEntity
{

    [MaxLength(250)]
    public string? Name { get; private set; }
    public string? Address { get; private set; }
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
    public CompanyRole? Role { get; private set; }
    public required Company Company { get; set; }

    public Agent() { }


    public Agent(
       string name,
       string address,
       string pan,
       string regNo,
       bool isParent,
       string referralCode,
       CompanyRole? role,
       Company company,
       Guid companyId

   )
    {
        Name = name;
        Address = address;
        Pan = pan;
        RegNo = regNo;
        IsParent = isParent;
        Company = company;
        CompanyId = companyId; // Initialize explicit CompanyId for tenant isolation
        ReferralCode = referralCode ?? CreateReferralCode(name);
        Role = role;
    }

    public void AddAgentRole(AgentRole role)
    {
        _rolesForUser.Add(role);
    }

    public void Update(
        string name,
        string address,
        string pan,
        string referralCode,
        string regNo,
        CompanyRole? role
    )
    {
        Name = name;
        Address = address;
        Pan = pan;
        ReferralCode = referralCode ?? CreateReferralCode(name);
        RegNo = regNo;
        Role = role;
    }

    public void AddUser(User user)
    {
        _agentUsers.Add(new AgentUser(user, this));
    }
    public string CreateReferralCode(string agentName)
    {
        var companyId = Company.Id.ToString();
        var regNo = RegNo?.Substring(0, 4) ?? "0000";
        var name = agentName.Replace(" ", "-").ToLower();
        var refName = name + companyId + regNo;
        return refName;
    }
}
