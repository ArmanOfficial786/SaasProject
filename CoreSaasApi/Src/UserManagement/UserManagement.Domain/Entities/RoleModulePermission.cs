namespace UserManagement.Domain.Entities;
//junction table for many to many relationship between Role and ModulePermission
public class RoleModulePermission
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission ModulePermission { get; private set; } = null!;

    // Constructor with objects
    public RoleModulePermission(Role role, ModulePermission modulePermission)
    {
        Role = role;
        ModulePermission = modulePermission;
        RoleId = role.Id;
        ModulePermissionId = modulePermission.Id;
    }

    // ✅ Add this constructor for seeding with IDs
    public RoleModulePermission(Guid roleId, Guid modulePermissionId)
    {
        RoleId = roleId;
        ModulePermissionId = modulePermissionId;
    }

#pragma warning disable CS8618
    private RoleModulePermission() { }
}

