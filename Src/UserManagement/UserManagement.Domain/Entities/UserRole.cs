namespace UserManagement.Domain.Entities;

public class UserRole : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }


    public Role? Role { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Role role, int? companyId = null)
    {
        UserId = userId;
        RoleId = role.Id;
        Role = role;
        CompanyId = companyId ?? role.CompanyId;
        SetEntry(userId);
    }

    public void Terminate() => SetTerminationDate();
}
