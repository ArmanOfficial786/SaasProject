namespace UserManagement.Domain.Entities;
//junction table for many to many relationship between Role and ModulePermission
public class RoleModulePermission
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission ModulePermission { get; private set; } = null!;

    public RoleModulePermission(Guid roleId, Guid modulePermissionId)
    {
        RoleId = roleId;
        ModulePermissionId = modulePermissionId;
    }

#pragma warning disable CS8618
    private RoleModulePermission() { }
}

