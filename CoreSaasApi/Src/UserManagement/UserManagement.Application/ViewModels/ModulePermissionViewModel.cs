using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.ViewModels;

public class ModulePermissionViewModel
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserModulePermission, ModulePermissionViewModel>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.ModulePermission!.Id))
                .ForMember(d => d.Code, o => o.MapFrom(s => s.ModulePermission!.ModuleId.ToString()))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.ModulePermission!.Permission.ToString()));
        }
    }
}
