//every user get their respective roles and permissions
//it is used to give exceptional permissions to a user for a specific module    
//i.e hr is on leave and this task is assigned to another user for a specific module


namespace UserManagement.Domain.Entities;

#pragma warning disable CA1711
//junction table for many to many relationship between User and ModulePermission
public class UserModulePermission : BaseEntity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission? ModulePermission { get; private set; }

    private UserModulePermission() { }

    public UserModulePermission(User user, ModulePermission modulePermission)
    {
        User = user;
        ModulePermission = modulePermission;
    }
}
#pragma warning restore CA1711
