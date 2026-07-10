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
    public List<string> RoleNames { get; set; } = [];

    public AgentViewModel() { }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Agent, AgentViewModel>()
                .ForMember(dest => dest.agentId, options => options.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoleNames, options => options.MapFrom(src => src.RolesForUser.Select(r => r.Role.Name).ToList()));

        }
    }

}

