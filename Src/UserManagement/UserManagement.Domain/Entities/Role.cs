



namespace UserManagement.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    // Tenant isolation - explicit CompanyId property
    public Guid CompanyId { get; private set; }

    [MaxLength(500)]
    public string Desc { get; private set; }
    public User? EntryBy { get; private set; }
    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    public void Terminate()
    {
        ToDate = DateTime.UtcNow;
    }

    private readonly List<RoleModulePermission> _roleModulePermissions = [];
    public IReadOnlyCollection<RoleModulePermission> RoleModulePermissions =>
        _roleModulePermissions.AsReadOnly();

    public Role(string name, string desc)
    {
        Name = name; // ✅ Name is inherited IdentityRole.Name
        Desc = desc;
    }

    public void AddRoleModulePermission(ModulePermission permission)
    {
        _roleModulePermissions.Add(new RoleModulePermission(this, permission));
    }

    public void RemoveRoleModulePermission(Guid modelPermissionId)
    {
        _ = _roleModulePermissions.Remove(_roleModulePermissions.Single(rmp => rmp.ModulePermissionId == modelPermissionId));
    }

    public void Update(string name, string desc)
    {
        Name = name;
        Desc = desc;
    }

}
