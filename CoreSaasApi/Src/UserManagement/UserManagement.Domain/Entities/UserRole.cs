namespace UserManagement.Domain.Entities;

public class UserRole : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }


    public Role? Role { get; private set; }
    public User? User { get; private set; }

    // Additional properties for role assignment status
    public bool IsActive { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? ApprovedDate { get; private set; }

    private UserRole() { }

    // Constructor with Role object (for runtime use)
    public UserRole(Guid userId, Role role, int? companyId = null)
    {
        UserId = userId;
        RoleId = role.Id;
        Role = role;
        CompanyId = companyId ?? role.CompanyId;
        SetEntry(userId);
        IsActive = true;
        IsApproved = true;
        IsDeleted = false;
        ApprovedDate = DateTime.UtcNow;
    }

    // Constructor with RoleId (for seeding)
    public UserRole(Guid userId, Guid roleId, int? companyId = null)
    {
        UserId = userId;
        RoleId = roleId;
        CompanyId = companyId;
        SetEntry(userId);
        IsActive = true;
        IsApproved = true;
        IsDeleted = false;
        ApprovedDate = DateTime.UtcNow;
    }

    public void Terminate()
    {
        SetTerminationDate();
        IsActive = false;
    }

    public new void Approve()
    {
        IsApproved = true;
        ApprovedDate = DateTime.UtcNow;
        base.Approve(); // calls AuditableEntity.Approve() to set VerificationStatus
    }

    public void SetUpdate(Guid userId)
    {
        base.SetUpdate(userId); // ✅ calls AuditableEntity.SetUpdate
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
    }
}
