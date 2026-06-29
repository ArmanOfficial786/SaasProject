using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class UserRole : BaseEntity
{
    public Role Role { get; private set; }

    public UserRole(Role role)
    {
        Role = role;
    }

#pragma warning disable CS8618
    private UserRole() { }
}
