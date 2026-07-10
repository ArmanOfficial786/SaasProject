

namespace UserManagement.Application.ViewModels;
//Display full details of a single Role (including nested permissions)
public class RoleViewModel
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Desc { get; init; }
    public List<ModulePermissionViewModel>? ModulePermissions { get; init; }

    public RoleViewModel() { }
}

public class Mapping : Profile
{
    public Mapping()
    {


        _ = CreateMap<Role, RoleViewModel>()
            .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Name!))
            .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Desc!))
            .ForMember(dest => dest.ModulePermissions, options => options.MapFrom(src => src.RoleModulePermissions));

        _ = CreateMap<AgentRole, RoleViewModel>()
            .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
            .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!))
            .ForMember(dest => dest.ModulePermissions, options => options.MapFrom(src => src.Role!.RoleModulePermissions));
    }

}
//Display summary information for multiple Roles (e.g., in a dropdown, table, or list)
public class RoleListViewModel
{
    public Guid Id { get; set; }
    public string? Name { get; init; }
    public string? Desc { get; init; }
    [JsonIgnore]
    public DateOnly? ToDate { get; private set; }
    public RoleListViewModel() { }

    public class Mapping : Profile
    {
        public Mapping()
        {
            //both entity and Dtos same so map it directly
            _ = CreateMap<Role, RoleListViewModel>();

            _ = CreateMap<AgentRole, RoleListViewModel>()
                .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!));
            _ = CreateMap<UserRole, RoleListViewModel>()
                .ForMember(dest => dest.Id, options => options.MapFrom(src => src.Role!.Id))
                .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!));
        }
    }
}
