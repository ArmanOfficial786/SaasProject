


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

    public UserModulePermission(Guid userId, Guid modulePermissionId)
    {
        UserId = userId;
        ModulePermissionId = modulePermissionId;
    }
}
#pragma warning restore CA1711
