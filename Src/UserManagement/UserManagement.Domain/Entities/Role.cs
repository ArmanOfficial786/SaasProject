



namespace UserManagement.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    //name id provided by IdentityRole<Guid>
    public int CompanyId { get; private set; }
    [MaxLength(500)]
    public string Desc { get; private set; }
    public User? EntryBy { get; private set; }
    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }

    // ✅ Navigation back to the Company
    public Company? Company { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private readonly List<RoleModulePermission> _roleModulePermissions = [];
    public IReadOnlyCollection<RoleModulePermission> RoleModulePermissions =>
        _roleModulePermissions.AsReadOnly();

    public Role(int companyId, string name, string desc)
    {
        CompanyId = companyId;
        Name = name; // ✅ Name is inherited IdentityRole.Name
        Desc = desc;
    }
    public void Terminate()
    {
        ToDate = DateTime.UtcNow;
    }


    public void AddRoleModulePermission(ModulePermission permission)
    {
        _roleModulePermissions.Add(new RoleModulePermission(Id, permission.Id));
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
