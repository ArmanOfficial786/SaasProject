


namespace UserManagement.Domain.Entities;

public class Role : IdentityRole<Guid>
{

    [MaxLength(500)]
    public string Desc { get; private set; }
    public User? EntryBy { get; private set; }
    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }

    // ✅ Navigation back to the Company
    public Company? Company { get; private set; }

    // ✅ CompanyId is NULL for global roles
    public int? CompanyId { get; private set; }

    public bool IsGlobalRole => CompanyId is null;
    public bool IsCompanyRole => CompanyId is not null;


    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RoleModulePermission> _roleModulePermissions = [];
    public IReadOnlyCollection<RoleModulePermission> RoleModulePermissions =>
        _roleModulePermissions.AsReadOnly();

    public Role(string name, string desc, int? companyId = null)
    {
        Name = name; // ✅ Name is inherited IdentityRole.Name
        Desc = desc;
        CompanyId = companyId;//null for global roles, otherwise set to the specific company ID
        ConcurrencyStamp = Guid.NewGuid().ToString();


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
        NormalizedName = name.ToUpperInvariant();
        // ✅ Update concurrency stamp on update
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }





}
