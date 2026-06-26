namespace UserManagement.Domain.Entities;

public class RoleModulePermission
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; }
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission ModulePermission { get; private set; }

    public RoleModulePermission(Role role, ModulePermission modulePermission)
    {
        Role = role;
        ModulePermission = modulePermission;
    }

#pragma warning disable CS8618
    private RoleModulePermission() { }
}

