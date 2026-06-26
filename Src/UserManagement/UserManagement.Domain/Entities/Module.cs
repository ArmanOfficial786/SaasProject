using System.ComponentModel.DataAnnotations.Schema;
using UserManagement.Domain.Enum;

namespace UserManagement.Domain.Entities;

public class Module
{
    public Guid Id { get; private set; }
    public ModuleEnum Code { get; private set; }
    public Guid ApplicationId { get; private set; }
    [MaxLength(100)]
    public string Name { get; private set; }
    [MaxLength(500)]
    public string Description { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }
    public Guid? MenuId { get; private set; }
    public Menu? Menu { get; private set; }

    private readonly List<Role> _roles = [];
    [NotMapped]
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private readonly List<ModulePermission> _ModulePermissions = [];
    public IReadOnlyCollection<ModulePermission> ModulePermissions => _ModulePermissions.AsReadOnly();

    public Module(Guid id, Guid applicationId, string name, string description, ModuleEnum code, DateTime fromDate, Guid? menuId = null)
    {
        Id = id;
        ApplicationId = applicationId;
        Name = name;
        Description = description;
        FromDate = fromDate;
        Code = code;
        MenuId = menuId;
    }

#pragma warning disable CS8618
    private Module() { }
}
