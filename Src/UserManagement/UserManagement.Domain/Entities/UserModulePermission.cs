using UserManagement.Domain.Entities;

namespace Security.Domain.Entities;
#pragma warning disable CA1711
//junction table for many to many relationship between User and ModulePermission
public class UserModulePermission
{
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission ModulePermission { get; private set; }

    public UserModulePermission(User user, ModulePermission modulePermission)
    {
        User = user;
        ModulePermission = modulePermission;
    }

#pragma warning disable CS8618
    private UserModulePermission() { }
}
