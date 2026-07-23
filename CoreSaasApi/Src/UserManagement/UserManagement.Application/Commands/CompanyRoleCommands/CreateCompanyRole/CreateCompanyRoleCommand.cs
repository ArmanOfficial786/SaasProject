namespace UserManagement.Application.Commands.RoleCommands.CreateRole;

public record CreateCompanyRoleCommand(string Name, string Desc, List<Guid> ModulePermissions) : IRequest<Response<RoleViewModel>>;

