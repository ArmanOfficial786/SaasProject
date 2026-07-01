using System.Text.Json.Serialization;

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
            .ForMember(x => x.Name, options => options.MapFrom(prop => prop.Role!.Name!))
            .ForMember(x => x.Desc, options => options.MapFrom(prop => prop.Role!.Desc!))
            .ForMember(x => x.ModulePermissions, options => options.MapFrom(prop => prop.Role!.RoleModulePermissions));

        _ = CreateMap<AgentRole, RoleViewModel>()
            .ForMember(x => x.Name, options => options.MapFrom(prop => prop.Role!.Name!))
            .ForMember(x => x.Desc, options => options.MapFrom(prop => prop.Role!.Desc!))
            .ForMember(x => x.ModulePermissions, options => options.MapFrom(prop => prop.Role!.RoleModulePermissions));
    }

}

public class RoleListViewModel
{
    public Guid Id { get; init; }
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
                .ForMember(x => x.Name, options => options.MapFrom(prop => prop.Role!.Name!))
                .ForMember(x => x.Desc, options => options.MapFrom(prop => prop.Role!.Desc!));
            _ = CreateMap<AgentRole, RoleListViewModel>()
                .ForMember(x => x.Name, options => options.MapFrom(prop => prop.Role!.Name!))
                .ForMember(x => x.Desc, options => options.MapFrom(prop => prop.Role!.Desc!));
            _ = CreateMap<UserRole, RoleListViewModel>()
                .ForMember(x => x.Id, options => options.MapFrom(prop => prop.Role!.Id))
                .ForMember(x => x.Name, options => options.MapFrom(prop => prop.Role!.Name!))
                .ForMember(x => x.Desc, options => options.MapFrom(prop => prop.Role!.Desc!));
        }
    }
}
