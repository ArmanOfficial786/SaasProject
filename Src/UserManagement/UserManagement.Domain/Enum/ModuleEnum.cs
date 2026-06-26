using System.ComponentModel;

namespace UserManagement.Domain.Enum;

public enum ModuleEnum
{
    [Description("Branch Role")]
    AgentRole,
    [Description("User Role")]
    UserRole,
    [Description("User")]
    User,
    //to be added in future

}

