using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class CompanyRole : AuditableEntity
{

    public Role? Role { get; private set; }

    public CompanyRole(Role role)
    {
        Role = role;
    }

    private CompanyRole() { }
}
