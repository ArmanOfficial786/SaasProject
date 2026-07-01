
using System.Text.Json.Serialization;
using Security.Domain.Entities;

namespace UserManagement.Application.ViewModels;

public class ModulePermissionViewModel
{
    public Guid Id { get; private set; }
    public string? ModuleName { get; private set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Permission Permission { get; private set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            _ = CreateMap<RoleModulePermission, ModulePermissionViewModel>()
                .ForMember(x => x.Id, options => options.MapFrom(prop => prop.ModulePermissionId))
                .ForMember(x => x.Permission, options => options.MapFrom(prop => prop.ModulePermission.Permission));

            _ = CreateMap<ModulePermission, ModulePermissionViewModel>();

            _ = CreateMap<UserModulePermission, ModulePermissionViewModel>()
                .ForMember(x => x.Id, options => options.MapFrom(prop => prop.ModulePermissionId))
                .ForMember(x => x.Permission, options => options.MapFrom(prop => prop.ModulePermission.Permission))
                .ForMember(x => x.ModuleName, options => options.MapFrom(prop => prop.ModulePermission.Module.Name))
                ;

        }
    }
}
