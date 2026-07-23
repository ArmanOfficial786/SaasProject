using AutoMapper;
using UserManagement.Domain.Entities;

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
    public List<RoleListViewModel> RoleList { get; set; } = [];
    public List<ModulePermissionViewModel> UserModulePermissionList { get; private set; } = [];

    public UserViewModel() { }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.UserModulePermissionList, options => options.MapFrom(src => src.UserModulePermissions))
                .ForMember(dest => dest.AgentId, options => options.MapFrom(src => src.AgentUsers.FirstOrDefault(au => au.ToDate == null)!.AgentId))
                .ForMember(dest => dest.AgentName, options => options.MapFrom(src => src.AgentUsers.FirstOrDefault(au => au.ToDate == null)!.Agent!.Name))
                .ForMember(dest => dest.RoleList, options => options.MapFrom(src => src.UserRoles.Where(ur => ur.ToDate == null)));
        }
    }
}

public record UserListViewModel
{
    public Guid Id { get; private set; }
    public string? UserName { get; private set; }
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public bool Active { get; private set; }

    public UserListViewModel() { }

    private class Mapping : Profile
    {
        public Mapping()
        {
            _ = CreateMap<User, UserListViewModel>()
                .ForMember(x => x.Active, options => options.MapFrom(u => u.UserStatuses.Any(us => us.ToDate == null)));
        }
    }
}
