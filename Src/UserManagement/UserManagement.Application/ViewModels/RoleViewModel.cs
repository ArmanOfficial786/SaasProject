

namespace UserManagement.Application.ViewModels;

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
        _ = CreateMap<Role, RoleViewModel>();

        _ = CreateMap<CompanyRole, RoleViewModel>()
            .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
            .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!))
            .ForMember(dest => dest.ModulePermissions, options => options.MapFrom(src => src.Role!.RoleModulePermissions));

        _ = CreateMap<AgentRole, RoleViewModel>()
            .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
            .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!))
            .ForMember(dest => dest.ModulePermissions, options => options.MapFrom(src => src.Role!.RoleModulePermissions));
    }

}

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
            _ = CreateMap<Role, RoleListViewModel>();
            _ = CreateMap<CompanyRole, RoleListViewModel>()
                .ForMember(dest => dest.Name, options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, options => options.MapFrom(src => src.Role!.Desc!));
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
