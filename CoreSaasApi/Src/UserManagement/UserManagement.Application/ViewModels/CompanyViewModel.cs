// File: CompanyViewModels.cs
// Namespace: UserManagement.Application.ViewModels
// Description: All ViewModels for Company entity and related entities (Role, User, Agent)
//              with AutoMapper profiles for mapping from Domain Entities.

namespace UserManagement.Application.ViewModels;

// ========================================================================
// 1. Lightweight list view for companies (grid, dropdown, search results)
// ========================================================================

/// <summary>
/// Lightweight ViewModel for listing companies.
/// Used in GET /api/companies (index/grid).
/// Contains only essential fields; no nested collections.
/// </summary>
public class CompanyListViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNo { get; set; }
    public string? Pan { get; set; }
    public string? RegNo { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            // All properties match by name – no explicit configuration needed
            CreateMap<Company, CompanyListViewModel>();
        }
    }
}

// ========================================================================
// 2. Detailed view of a single company (including related entities)
// ========================================================================

/// <summary>
/// Detailed ViewModel for a single company, including nested collections (Roles, Users, Agents).
/// Used in GET /api/companies/{id}.
/// </summary>
public class CompanyDetailViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhoneNo { get; set; }
    public string? Pan { get; set; }
    public string? RegNo { get; set; }
    public string? Url { get; set; }

    // Nested collections (also ViewModels)
    public List<RoleListViewModel> Roles { get; set; } = new();
    public List<UserListViewModel> Users { get; set; } = new();
    //public List<AgentListViewModel> Agents { get; set; } = new();

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Company, CompanyDetailViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users));
            // .ForMember(dest => dest.Agents, opt => opt.MapFrom(src => src.Agents));
        }
    }
}

// ========================================================================
// 3. Create new company (POST)
// ========================================================================

/// <summary>
/// ViewModel for creating a new company.
/// Used in POST /api/companies.
/// Contains only the fields that the client can provide.
/// </summary>
public class CompanyCreateViewModel
{

    public string? Name { get; set; }
    public string? ProductCode { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhoneNo { get; set; }
    public string? Pan { get; set; }
    public string? RegNo { get; set; }
    public string? Url { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? MainUsername { get; set; }
    public string? MainUserFirstName { get; set; }
    public string? MainUserLastName { get; set; }
    public string? MainUserEmail { get; set; }
    public string? MainUserContactNo { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            // Use constructor to create Company with provided values
            CreateMap<CompanyCreateViewModel, Company>();
            // Map Company back to CompanyCreateViewModel for response
            CreateMap<Company, CompanyCreateViewModel>();

        }
    }
}

// ========================================================================
// 4. Update existing company (PUT)
// ========================================================================

/// <summary>
/// ViewModel for updating an existing company.
/// Used in PUT /api/companies/{id}.
/// Includes Id to identify the record.
/// </summary>
public class CompanyUpdateViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? PhoneNo { get; set; }
    public string? Pan { get; set; }
    public string? RegNo { get; set; }


    public string? Url { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CompanyUpdateViewModel, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())          // Id is read-only
                .ForMember(dest => dest.Roles, opt => opt.Ignore())      // Don't map navigations
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.Agents, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore()) // Not exposed
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNo, opt => opt.MapFrom(src => src.PhoneNo))
                .ForMember(dest => dest.Pan, opt => opt.MapFrom(src => src.Pan))
                .ForMember(dest => dest.RegNo, opt => opt.MapFrom(src => src.RegNo))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));
        }
    }
}

// ========================================================================
// 5. Ultra-lightweight for dropdown selections
// ========================================================================

/// <summary>
/// Ultra-lightweight ViewModel for dropdown lists.
/// Used in GET /api/companies/dropdown.
/// Contains only Id and Name.
/// </summary>
public class CompanyDropdownViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Company, CompanyDropdownViewModel>();
        }
    }
}

// ========================================================================
// 6. Company with its Roles (for role-assignment screens)
// ========================================================================

/// <summary>
/// ViewModel for a company with its roles.
/// Useful for role assignment screens.
/// Used in GET /api/companies/{id}/roles.
/// </summary>
public class CompanyWithRolesViewModel
{
    public int Id { get; set; }
    public string? ProductCode { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNo { get; set; }
    public List<RoleListViewModel> Roles { get; set; } = new();

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Company, CompanyWithRolesViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));
        }
    }
}

// ========================================================================
// 7. Company with its Branches (Agents)
// ========================================================================

/// <summary>
/// ViewModel for a company with its branches (Agents).
/// Useful for branch management screens.
/// Used in GET /api/companies/{id}/agents.
/// </summary>
//public class CompanyWithAgentsViewModel
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public string? Email { get; set; }
//    public string? PhoneNo { get; set; }
//    public List<AgentListViewModel> Agents { get; set; } = new();

//    public class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<Company, CompanyWithAgentsViewModel>()
//                .ForMember(dest => dest.Agents, opt => opt.MapFrom(src => src.Agents));
//        }
//    }
//}

// ========================================================================
// Supporting ViewModels for nested collections
// ========================================================================

/// <summary>
/// Lightweight ViewModel for Role, used in lists and nested collections.
/// </summary>
//public class RoleListViewModel
//{
//    public Guid Id { get; set; }
//    public string? Name { get; set; }
//    public string? Desc { get; set; }
//    public DateTime? ToDate { get; set; } // For soft-delete/termination

//    public class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<Role, RoleListViewModel>();
//        }
//    }
//}

/// <summary>
/// Lightweight ViewModel for User, used in lists and nested collections.
/// </summary>
//public class UserListViewModel
//{
//    public Guid Id { get; set; }
//    public string? FirstName { get; set; }
//    public string? LastName { get; set; }
//    public string? Email { get; set; }
//    public string? Contact { get; set; }
//    public bool IsActive { get; set; }

//    public class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<User, UserListViewModel>();
//        }
//    }
//}

/// <summary>
/// Lightweight ViewModel for Agent (Branch), used in lists and nested collections.
/// </summary>
//public class AgentListViewModel
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public string? Address { get; set; }
//    public string? Pan { get; set; }
//    public string? RegNo { get; set; }
//    public bool IsParent { get; set; }
//    public string? ReferralCode { get; set; }

//    public class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<Agent, AgentListViewModel>();
//        }
//    }
//}

// ========================================================================
// Optional: Consolidated Profile (if you prefer to have all mappings in one place)
// You can use this instead of the nested Mapping classes inside each ViewModel.
// For simplicity, we keep the nested ones, but you can uncomment and use this.
// ========================================================================

//public class CompanyProfile : Profile
//{
//    public CompanyProfile()
//    {
//        CreateMap<Company, CompanyListViewModel>();
//        CreateMap<Company, CompanyDetailViewModel>()
//            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
//            .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users))
//            .ForMember(dest => dest.Agents, opt => opt.MapFrom(src => src.Agents));
//        CreateMap<CompanyCreateViewModel, Company>()
//            .ConstructUsing(src => new Company(
//                src.Name ?? string.Empty,
//                src.Email ?? string.Empty,
//                src.Address ?? string.Empty,
//                src.PhoneNo ?? string.Empty,
//                src.Pan ?? string.Empty,
//                src.RegNo ?? string.Empty,
//                src.Url ?? string.Empty
//            ));
//        CreateMap<CompanyUpdateViewModel, Company>()
//            .ForMember(dest => dest.Id, opt => opt.Ignore())
//            .ForMember(dest => dest.Roles, opt => opt.Ignore())
//            .ForMember(dest => dest.Users, opt => opt.Ignore())
//            .ForMember(dest => dest.Agents, opt => opt.Ignore())
//            .ForMember(dest => dest.ProductCode, opt => opt.Ignore());
//        CreateMap<Company, CompanyDropdownViewModel>();
//        CreateMap<Company, CompanyWithRolesViewModel>()
//            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));
//        CreateMap<Company, CompanyWithAgentsViewModel>()
//            .ForMember(dest => dest.Agents, opt => opt.MapFrom(src => src.Agents));
//        CreateMap<Role, RoleListViewModel>();
//        CreateMap<User, UserListViewModel>();
//        CreateMap<Agent, AgentListViewModel>();
//    }
//}
