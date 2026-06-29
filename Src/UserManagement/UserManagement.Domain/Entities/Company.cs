//it's a company

using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class Company : AuditableEntity
{
    public string? ProductCode { get; private set; }
    public string? Name { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNo { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public string? Url { get; private set; }


    //Navigation
    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<CompanyRole> _rolesForUser = [];
    public IReadOnlyCollection<CompanyRole> RolesForUser => _rolesForUser.AsReadOnly();

    public Company(string name, string email, string address, string phoneNo, string pan, string regNo, string url)
    {
        Name = name;
        Email = email;
        Address = address;
        PhoneNo = phoneNo;
        Pan = pan;
        RegNo = regNo;
        Url = url;
    }

    public void AddTenantRole(CompanyRole role)
    {
        _rolesForUser.Add(role);
    }

    public void RemoveTenantRole(CompanyRole role)
    {
        _rolesForUser.Remove(role);
    }

    public void AddAgent(User agent)
    {
        _users.Add(agent);
    }
    private Company() { } // For EF Core

}
