using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Domain.Entities;

public class ModulePermission
{
    public Guid Id { get; private set; }
    public Guid ModuleId { get; private set; }
    public Module Module { get; private set; } = null!;
    public Permission Permission { get; private set; }

    private readonly List<Role> _roles = [];
    [NotMapped]
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public ModulePermission(Guid id, Guid moduleId, Permission permission)
    {
        Id = id;
        ModuleId = moduleId;
        Permission = permission;
    }

#pragma warning disable CS8618
    private ModulePermission() { }
}

