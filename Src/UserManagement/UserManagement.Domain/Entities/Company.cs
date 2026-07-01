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

    private readonly List<CompanyRole> _rolesForUser = [];
    public IReadOnlyCollection<CompanyRole> RolesForUser => _rolesForUser.AsReadOnly();

    private readonly List<Agent> _agents = [];
    public IReadOnlyCollection<Agent> Agents => _agents.AsReadOnly();

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

    public void AddCompanyRole(CompanyRole role)
    {
        _rolesForUser.Add(role);
    }

    public void RemoveCompanyRole(CompanyRole role)
    {
        _rolesForUser.Remove(role);
    }

    public void AddAgent(Agent agent)
    {
        _agents.Add(agent);
    }
    private Company() { } // For EF Core

}
