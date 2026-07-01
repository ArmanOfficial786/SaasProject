namespace UserManagement.Application.ViewModels;

public class UserViewModel
{
    public Guid Id { get; private set; }
    public string? UserName { get; private set; }
    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? LastName { get; private set; }
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public string? Contact { get; private set; }
    public Guid AgentId { get; private set; }
    public string? AgentName { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public List<ModulePermissionViewModel> UserModulePermissionList { get; private set; } = [];
    public List<RoleListViewModel> RoleList { get; set; } = [];

    public UserViewModel() { }

    private class Mapping : Profile
    {
        public Mapping()
        {      //CreateMap<TSource, TDestination>() is a method provided by AutoMapper that defines a mapping between two types. In this case, it is mapping from the User entity to the UserViewModel.
            _ = CreateMap<User, UserViewModel>()
                .ForMember(x => x.UserModulePermissionList, options => options.MapFrom(prop => prop.UserPermissions))
                .ForMember(x => x.AgentId, options => options.MapFrom(prop => prop.AgentUsers.FirstOrDefault(x => x.ToDate == null)!.Agent.Id))
                .ForMember(x => x.AgentName, options => options.MapFrom(prop => prop.AgentUsers.FirstOrDefault(x => x.ToDate == null)!.Agent.Name))
                .ForMember(x => x.RoleList, options => options.MapFrom(prop => prop.UserRoles.Where(x => x.ToDate == null)));
        }
    }
}
