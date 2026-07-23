using System.Reflection;
using UserManagement.Domain.Enum;

namespace Security.Domain.Entities;

public class Application
{
    public Guid Id { get; private set; }
    public ApplicationEnum Code { get; private set; }
    [MaxLength(100)]
    public string Name { get; private set; }
    [MaxLength(500)]
    public string Desc { get; private set; }

    private readonly List<Module> _modules = [];
    public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();

    public Application(Guid id, string name, string desc, ApplicationEnum code)
    {
        Id = id;
        Name = name;
        Desc = desc;
        Code = code;
    }
}
