namespace UserManagement.Domain.Entities;

public class UserRole : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }


    public Role? Role { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Role role)
    {
        UserId = userId;
        RoleId = role.Id;
        Role = role;
        // FIX #4: junction entity sets its own audit on creation
        SetEntry(userId);
    }

    public void Terminate() => SetTerminationDate();
}
