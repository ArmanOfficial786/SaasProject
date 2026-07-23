//it's a company

namespace UserManagement.Domain.Entities;

public class Company
{
    public int Id { get; private set; }
    public string? ProductCode { get; private set; }
    public string? Name { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNo { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public string? Url { get; private set; }


    //Navigation

    private readonly List<CompanyRole> _rolesForAgent = [];
    public IReadOnlyCollection<CompanyRole> RolesForAgent => _rolesForAgent.AsReadOnly();

    // ✅ Navigation: One Company has many Roles (One‑to‑Many)
    private readonly List<Role> _roles = [];
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<Agent> _agents = [];
    public IReadOnlyCollection<Agent> Agents => _agents.AsReadOnly();

    public Company(string productCode, string name, string email, string address, string phoneNo, string pan, string regNo, string url)
    {
        ProductCode = productCode;
        Name = name;
        Email = email;
        Address = address;
        PhoneNo = phoneNo;
        Pan = pan;
        RegNo = regNo;
        Url = url;
    }

    // ✅ Factory method for seed data — allowed to set Id because it's a member of this class
    public static Company CreateSeed(int id, string productCode, string name, string email,
        string address, string phoneNo, string pan, string regNo, string url)
    {
        return new Company(productCode, name, email, address, phoneNo, pan, regNo, url)
        {
            Id = id
        };
    }


    // ✅ Business methods for managing Roles
    public void AddCompanyRole(CompanyRole role)
    {
        _rolesForAgent.Add(role);
    }

    public void RemoveRole(Role role)
    {
        _roles.Remove(role);
    }

    public void AddAgent(Agent agent)
    {
        _agents.Add(agent);
    }
    private Company() { } // For EF Core

}
