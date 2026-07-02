namespace UserManagement.Application.ViewModels;

public class AgentViewModel
{
    public Guid agentId { get; private set; }
    public string? Name { get; private set; }
    public string? Address { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public bool IsParent { get; private set; }
    public string? ReferralCode { get; private set; }
    public Guid RoleId { get; private set; }
    public string? RoleName { get; private set; }

    public AgentViewModel() { }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Agent, AgentViewModel>()
                .ForMember(dest => dest.agentId, options => options.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoleId, options => options.MapFrom(src => src.Role != null ? src.Role.Id : Guid.Empty))
                .ForMember(dest => dest.RoleName, options => options.MapFrom(src => src.Role != null ? src.Role!.Role!.Name : string.Empty));
        }
    }

}

