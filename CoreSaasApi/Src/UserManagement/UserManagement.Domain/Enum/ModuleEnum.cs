using System.ComponentModel;

namespace UserManagement.Domain.Enum;

public enum ModuleEnum
{
    [Description("Company Role")]
    CompanyRole = 1,
    [Description("Branch Role")]
    AgentRole = 2,
    [Description("User Role")]
    UserRole = 3,
    [Description("User")]
    User = 4,

    //to be added in future

}

