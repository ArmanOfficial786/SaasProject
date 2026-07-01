using UserManagement.Domain.Entities.BaseEntities;
//junction table for many to many relationship between Company and Role
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
