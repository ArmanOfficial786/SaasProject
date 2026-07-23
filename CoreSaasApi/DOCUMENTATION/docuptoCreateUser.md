# CoreSaas Project - CreateUser Feature Documentation

## Table of Contents
1. [Project Structure](#project-structure)
2. [Architecture Overview](#architecture-overview)
3. [Implementation Steps](#implementation-steps)
4. [Code Implementation](#code-implementation)
5. [Database Models](#database-models)
6. [Event System](#event-system)
7. [Error Handling](#error-handling)
8. [Configuration](#configuration)

---

## Project Structure

```
D:\ARMAN\SaasProject\
├── Src/
│   ├── Shared/
│   │   ├── Shared.Domain/
│   │   │   ├── DTOs/
│   │   │   │   ├── UserInfo.cs
│   │   │   │   ├── Response.cs
│   │   │   │   └── ErrorDTO.cs
│   │   │   └── Abstraction/
│   │   ├── Shared.Application/
│   │   │   └── Interface/
│   │   │       └── ICurrentUserService.cs
│   │   └── Shared.Infrastructure/
│   │       ├── Service/
│   │       │   └── CurrentUserService.cs
│   │       └── Data/Configurations/
│   │           └── UserConfiguration.cs
│   ├── UserManagement/
│   │   ├── UserManagement.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── UserRole.cs
│   │   │   │   ├── Company.cs
│   │   │   │   ├── Agent.cs
│   │   │   │   └── BaseEntities/
│   │   │   │       ├── AuditableEntity.cs
│   │   │   │       └── BaseEvent.cs
│   │   │   ├── Events/
│   │   │   │   └── user/
│   │   │   │       └── UserCreatedEvent.cs
│   │   │   └── Exceptions/
│   │   ├── UserManagement.Application/
│   │   │   ├── Commands/
│   │   │   │   └── UserCommands/
│   │   │   │       └── CreateUser/
│   │   │   │           ├── CreateUserCommand.cs
│   │   │   │           └── CreateUserCommandHandler.cs
│   │   │   └── ViewModels/
│   │   │       ├── UserViewModel.cs
│   │   │       └── RoleViewModel.cs
│   │   └── UserManagement.Infrastructure/
│   ├── Product/
│   │   ├── Hrm.Api/
│   │   └── School.Api/
│   └── UserManagement.Api/
└── CoreSaas.slnx (Solution file)
```

---

## Architecture Overview

The CreateUser feature follows a **Clean Architecture** pattern with:
- **Domain Layer**: Core business entities and rules
- **Application Layer**: CQRS commands and DTOs
- **Infrastructure Layer**: Data access and services
- **Presentation Layer**: API controllers

### Flow Diagram
```
API Request (POST /users/create)
    ↓
CreateUserCommand (Request DTO)
    ↓
CreateUserCommandHandler (MediatR Handler)
    ↓
Domain Entities & Business Logic
    ↓
Unit of Work (Repository Pattern)
    ↓
Database Operations
    ↓
UserCreatedEvent (Domain Event)
    ↓
Response<UserViewModel> (Response DTO)
```

---

## Implementation Steps

### Step 1: Define Domain Models

#### 1.1 Company Entity
```csharp
namespace UserManagement.Domain.Entities;

public class Company
{
    public int Id { get; private set; }
    public string? ProductCode { get; private set; }
    public string? Name { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNo { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public string? Url { get; private set; }

    private List<Agent> _agents = [];
    public IReadOnlyCollection<Agent> Agents => _agents.AsReadOnly();

    public Company(string name, string email, string address, string phoneNo, 
                   string pan, string regNo, string url)
    {
        Name = name;
        Email = email;
        Address = address;
        PhoneNo = phoneNo;
        Pan = pan;
        RegNo = regNo;
        Url = url;
    }

    private Company() { }
}
```

#### 1.2 Agent Entity
```csharp
namespace UserManagement.Domain.Entities;

public class Agent : AuditableEntity
{
    public string? Name { get; private set; }
    public string? Address { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public bool IsParent { get; private set; }
    public string? ReferralCode { get; private set; }

    public Company Company { get; private set; }
    public CompanyRole? Role { get; private set; }

    private List<AgentUser> _agentUsers = [];
    public IReadOnlyCollection<AgentUser> AgentUsers => _agentUsers.AsReadOnly();

    private List<AgentRole> _agentRoles = [];
    public IReadOnlyCollection<AgentRole> AgentRoles => _agentRoles.AsReadOnly();

    public Agent(
        string name,
        string address,
        string pan,
        string regNo,
        bool isParent,
        string referralCode,
        CompanyRole? role,
        Company company,
        int companyId
    )
    {
        Name = name;
        Address = address;
        Pan = pan;
        RegNo = regNo;
        IsParent = isParent;
        Company = company;
        CompanyId = companyId;
        ReferralCode = referralCode ?? CreateReferralCode(name);
        Role = role;
    }

    private string CreateReferralCode(string name) => 
        $"REF_{name}_{Guid.NewGuid().ToString().Substring(0, 8)}";

    private Agent() { }

    public void AddAgentRole(AgentRole role) => _agentRoles.Add(role);
}
```

#### 1.3 Base Auditable Entity
```csharp
namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class AuditableEntity : BaseEntity
{
    // Tenant isolation - explicit CompanyId property
    public int CompanyId { get; set; }

    // Foreign keys for audit trail
    public Guid? EntryByUserId { get; private set; }
    public User? EntryBy { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }
    public User? UpdatedBy { get; private set; }

    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = 
        VerificationStatus.Saved;

    public void SetEntry(User? entryBy)
    {
        EntryBy = entryBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void SetUpdate(User? updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Submit() => VerificationStatus = VerificationStatus.Submitted;

    public void Approve() => VerificationStatus = VerificationStatus.Approved;

    public void Reject() => VerificationStatus = VerificationStatus.Rejected;

    protected void SetTerminationDate(DateTime? date = null) =>
        ToDate = date ?? DateTime.UtcNow;

    public bool IsTerminated => ToDate != null;
    public bool IsVerified => VerificationStatus == VerificationStatus.Approved;
    public bool IsRejected => VerificationStatus == VerificationStatus.Rejected;
    public bool IsUnapproved => VerificationStatus == VerificationStatus.Submitted;

    public bool ValidOnDate(DateTime date) => ValidOnDate(DateOnly.FromDateTime(date));

    public bool ValidOnDate(DateOnly date)
    {
        if (!IsVerified) return false;
        return date >= DateOnly.FromDateTime(EntryDate) && 
               (ToDate == null || date <= DateOnly.FromDateTime(ToDate.Value));
    }
}
```

#### 1.4 User Entity (Core Domain Model)
```csharp
namespace UserManagement.Domain.Entities;

public class User : IdentityUser<Guid>
{
    [MaxLength(256)]
    public new string? Email { get; private set; }

    [MaxLength(30)]
    public string? FirstName { get; private set; }

    [MaxLength(30)]
    public string? MiddleName { get; private set; }

    [MaxLength(30)]
    public string? LastName { get; private set; }

    public string? FullName =>
        FirstName + " " + (!String.IsNullOrEmpty(MiddleName) ? MiddleName + " " : "") + LastName;

    [MaxLength(256)]
    public string? Contact { get; private set; }

    [MaxLength(256)]
    public new string? PasswordHash { get; private set; }

    public bool IsEmailConfirmed { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public User? EntryBy { get; private set; }
    public DateTime EntryDate { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Tenant isolation
    public int CompanyId { get; private set; }
    public Company? Company { get; private set; }

    public bool IsSuperAdmin { get; private set; }

    // Collections
    private List<UserModulePermission> _userModulePermissions = [];
    public IReadOnlyCollection<UserModulePermission> UserModulePermissions =>
        _userModulePermissions.AsReadOnly();

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserStatus> _userStatuses = [];
    public IReadOnlyCollection<UserStatus> UserStatuses => _userStatuses.AsReadOnly();

    private readonly List<AgentUser> _agentUsers = [];
    public IReadOnlyCollection<AgentUser> AgentUsers => _agentUsers.AsReadOnly();

    // Constructor
    public User(
        Company company,
        string userName,
        string firstName,
        string? middleName,
        string lastName,
        string email,
        string? contact,
        User? entryBy
    )
    {
        Id = Guid.NewGuid();
        UserName = userName;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        Contact = contact;
        Company = company;
        CompanyId = company.Id;
        EntryBy = entryBy;
        EntryDate = DateTime.UtcNow;
        AddStatus(new(null));
    }

    // EF Core parameterless constructor
    private User() { }

    // Business Methods
    private void AddStatus(UserStatus status) => _userStatuses.Add(status);

    public void AddRole(Role role) => _userRoles.Add(new UserRole(role));

    public void RemoveRole(Guid roleId)
    {
        var role = _userRoles.Single(x => x.Role.Id == roleId && x.ToDate == null);
        role.Terminate();
    }

    public void AddToAgent(Agent agent)
    {
        _agentUsers.ForEach(bu => bu.Terminate());
        _agentUsers.Add(new AgentUser(this, agent));
    }

    public void AddModulePermission(ModulePermission modulePermission)
    {
        _userModulePermissions.Add(new UserModulePermission(this, modulePermission));
    }

    public void RemoveModulePermission(Guid modulePermissionId)
    {
        _ = _userModulePermissions.Remove(
            _userModulePermissions.Single(x => x.ModulePermissionId == modulePermissionId)
        );
    }

    public void Terminate(string? remarks)
    {
        var currStatus = _userStatuses.SingleOrDefault(x => x.ToDate == null);
        if (currStatus != null)
        {
            currStatus.Terminate(remarks);
        }
    }

    public void Update(
        string userName,
        string firstName,
        string? middleName,
        string lastName,
        string email,
        string? contact,
        User? entryBy
    )
    {
        UserName = userName;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        Contact = contact;
        EntryBy = entryBy;
    }
}
```

#### 1.5 UserRole Entity
```csharp
namespace UserManagement.Domain.Entities;

public class UserRole : AuditableEntity
{
    public Role Role { get; private set; }

    public UserRole(Role role)
    {
        Role = role;
    }

    public void Terminate()
    {
        SetTerminationDate();
    }

#pragma warning disable CS8618
    private UserRole() { }
}
```

---

### Step 2: Define DTOs and Request Models

#### 2.1 UserInfo DTO (Shared Layer)
```csharp
namespace Shared.Domain.DTOs;

public class UserInfo(Guid id, string userName, string name, int companyId)
{
    public Guid Id { get; set; } = id;
    public string UserName { get; set; } = userName;
    public string Name { get; set; } = name;
    public int CompanyId { get; set; } = companyId;
}
```

#### 2.2 Response DTO
```csharp
namespace Shared.Domain.DTOs;

public class Response<T> where T : class
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<ErrorDTO> Errors { get; set; } = [];
    public T? Data { get; set; }

    public static Response<T> SuccessResponse(T data, string? msg = null)
    {
        return new()
        {
            Success = true,
            Message = msg,
            Data = data
        };
    }

    public static Response<T> SuccessResponse(string msg)
    {
        return new()
        {
            Success = true,
            Message = msg
        };
    }

    public static Response<T> FailureResponse(params ErrorDTO[] errors)
    {
        return new()
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
}
```

#### 2.3 Error DTO
```csharp
namespace Shared.Domain.DTOs;

public record ErrorDTO(string Code, string? Message)
{
}
```

#### 2.4 CreateUserCommand
```csharp
namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public class CreateUserCommand : IRequest<Response<UserViewModel>>
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public List<Guid> Roles { get; set; } = [];
    public List<Guid> ModulePermissions { get; set; } = [];
}
```

---

### Step 3: Create Application Services

#### 3.1 ICurrentUserService Interface
```csharp
namespace Shared.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    UserInfo? UserInfo { get; }
    int? CompanyId { get; }
    Guid? AgentId { get; }
    Guid? BranchId { get; }
    Guid? CustomerId { get; }
}
```

#### 3.2 CurrentUserService Implementation
```csharp
namespace Shared.Infrastructure.Service;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = GetClaimValue("UserId");
            return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
        }
    }

    public string? UserName
    {
        get
        {
            var userName = GetClaimValue("UserName");
            return string.IsNullOrEmpty(userName) ? null : userName;
        }
    }

    public int? CompanyId
    {
        get
        {
            var companyId = GetClaimValue("CompanyId");
            try
            {
                return string.IsNullOrEmpty(companyId) ? null : int.Parse(companyId);
            }
            catch
            {
                return null;
            }
        }
    }

    public Guid? AgentId => null;
    public Guid? BranchId => null;

    public Guid? CustomerId
    {
        get
        {
            var customerId = GetClaimValue("CustomerId");
            try
            {
                return string.IsNullOrEmpty(customerId) ? null : Guid.Parse(customerId);
            }
            catch
            {
                return null;
            }
        }
    }

    public UserInfo? UserInfo
    {
        get
        {
            if (UserId.HasValue && !string.IsNullOrEmpty(UserName))
            {
                var companyIdStr = GetClaimValue("CompanyId");
                var companyId = 0;
                if (!string.IsNullOrEmpty(companyIdStr) && int.TryParse(companyIdStr, out var id))
                {
                    companyId = id;
                }
                return new(UserId ?? Guid.Empty, UserName, GetClaimValue("Name")!, companyId);
            }
            else
                return null;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        try
        {
            return (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                ? _httpContextAccessor.HttpContext?.User?.Claims
                    .Single(c => c.Type == claimType).Value ?? null
                : null;
        }
        catch
        {
            return null;
        }
    }
}
```

---

### Step 4: Create Command Handler

#### 4.1 CreateUserCommandHandler
```csharp
using System.Web;
using UserManagement.Domain.Events.user;
using Shared.Domain.DTOs;

namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;
    private readonly MailConfig _mailConfig;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService, 
        UserManager<User> userManager, 
        IMediator mediator, 
        MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _mediator = mediator;
        _mailConfig = mailConfig;
    }

    public async Task<Response<UserViewModel>> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get current user information
            var userInfo = _currentUserService.UserInfo ?? 
                throw new UnauthorizedAccessException();
            var loggedInUserId = userInfo.Id;
            var companyId = userInfo.CompanyId;
            var agentId = _currentUserService.AgentId ?? 
                throw new UnauthorizedAccessException();

            // 2. Validate roles
            if (request.Roles == null || request.Roles.Count < 1)
            {
                return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);
            }

            // 3. Get repositories
            var userRepo = _unitOfWork.Repository<User>();
            var companyRepo = _unitOfWork.Repository<Company>();
            var agentRepo = _unitOfWork.Repository<Agent>();
            var agentRoleRepo = _unitOfWork.Repository<AgentRole>();
            var userStatusRepo = _unitOfWork.Repository<UserStatus>();
            var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();

            // 4. Fetch related entities
            var entryBy = await userRepo.GetSingleOrDefaultAsync(
                x => x.Id == loggedInUserId, 
                cancellationToken: cancellationToken);

            var company = await companyRepo.GetSingleOrDefaultAsync(
                x => x.Id == companyId, 
                cancellationToken: cancellationToken);

            if (company == null)
            {
                return Response<UserViewModel>.FailureResponse(
                    Errors.AgentNotFoundForBranch);
            }

            var agent = await agentRepo.GetSingleOrDefaultAsync(
                x => x.Id == agentId, 
                cancellationToken: cancellationToken);

            if (agent == null)
            {
                return Response<UserViewModel>.FailureResponse(
                    Errors.AgentNotFoundForBranch);
            }

            // 5. Create user domain entity
            User user = new(
                company,
                request.UserName,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.Email,
                request.Contact,
                entryBy
            );

            user.AddToAgent(agent);

            // 6. Add roles
            var agentRoles = agentRoleRepo
                .GetAll(x => request.Roles.Contains(x.Id), null, x => x.Role)
                .ToList();

            if (agentRoles == null || agentRoles.Count < 1)
            {
                return Response<UserViewModel>.FailureResponse(
                    Errors.RoleIsRequired);
            }

            agentRoles.ForEach(x => user.AddRole(x.Role));

            // 7. Add module permissions
            var permissionsToAdd = await modulePermissionRepo.GetListAsync(
                x => request.ModulePermissions.Contains(x.Id),
                null,
                false,
                cancellationToken
            );
            permissionsToAdd.ForEach(x => user.AddModulePermission(x));

            // 8. Create user in identity system
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return Response<UserViewModel>.FailureResponse(
                    result
                        .Errors.Select(err => new ErrorDTO(err.Code, err.Description))
                        .ToArray()
                );
            }

            // 9. Generate password reset token
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string encodedToken = HttpUtility.UrlEncode(token);
            string encodedEmail = HttpUtility.UrlEncode(request.Email);

            // 10. Publish domain event
            var evt = new UserCreatedEvent(
                user.FullName!,
                user.UserName,
                user.Email!,
                _mailConfig.OfficeURL +
                    string.Format(_mailConfig.OfficeNewUserUrl, encodedToken, encodedEmail)
            );
            await _mediator.Publish(evt);

            // 11. Save changes
            _ = await _unitOfWork.SaveChangesAsync();

            // 12. Map to DTO
            var userVM = _mapper.Map<UserViewModel>(user);
            foreach (var role in userVM.RoleList)
            {
                role.Id = agentRoles.Single(x => x.Role.Id == role.Id).Id;
            }

            return Response<UserViewModel>.SuccessResponse(userVM);
        }
        catch (Exception ex)
        {
            return Response<UserViewModel>.FailureResponse(
                Errors.Exception(ex));
        }
    }
}
```

---

### Step 5: Define Domain Events

#### 5.1 UserCreatedEvent
```csharp
namespace UserManagement.Domain.Events.user;

public class UserCreatedEvent : BaseEvent
{
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? ResetPasswordUrl { get; set; }

    public UserCreatedEvent(
        string? fullName, 
        string? userName, 
        string? email, 
        string? resetPasswordUrl)
    {
        FullName = fullName;
        UserName = userName;
        Email = email;
        ResetPasswordUrl = resetPasswordUrl;
    }
}
```

#### 5.2 BaseEvent
```csharp
namespace UserManagement.Domain.Entities.BaseEntities;

public abstract class BaseEvent : INotification
{
}
```

---

### Step 6: Create ViewModels

#### 6.1 UserViewModel with AutoMapper Profile
```csharp
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
            _ = CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.UserModulePermissionList, 
                    options => options.MapFrom(src => src.UserModulePermissions))
                .ForMember(dest => dest.AgentId, 
                    options => options.MapFrom(src => 
                        src.AgentUsers.FirstOrDefault(dest => dest.ToDate == null)!.Agent.Id))
                .ForMember(dest => dest.AgentName, 
                    options => options.MapFrom(src => 
                        src.AgentUsers.FirstOrDefault(dest => dest.ToDate == null)!.Agent.Name))
                .ForMember(dest => dest.RoleList, 
                    options => options.MapFrom(src => 
                        src.UserRoles.Where(dest => dest.ToDate == null)));
        }
    }
}
```

#### 6.2 RoleListViewModel with AutoMapper Profile
```csharp
namespace UserManagement.Application.ViewModels;

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
                .ForMember(dest => dest.Name, 
                    options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, 
                    options => options.MapFrom(src => src.Role!.Desc!));

            _ = CreateMap<AgentRole, RoleListViewModel>()
                .ForMember(dest => dest.Name, 
                    options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, 
                    options => options.MapFrom(src => src.Role!.Desc!));

            _ = CreateMap<UserRole, RoleListViewModel>()
                .ForMember(dest => dest.Id, 
                    options => options.MapFrom(src => src.Role!.Id))
                .ForMember(dest => dest.Name, 
                    options => options.MapFrom(src => src.Role!.Name!))
                .ForMember(dest => dest.Desc, 
                    options => options.MapFrom(src => src.Role!.Desc!));
        }
    }
}
```

---

### Step 7: Configure Database Entities

#### 7.1 UserConfiguration
```csharp
namespace Shared.Infrastructure.Data.Configurations.UserManagement;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UserId");

        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Contact).HasMaxLength(256);
        builder.Property(u => u.PasswordHash).HasMaxLength(256);

        // Tenant isolation
        builder.Property(u => u.CompanyId).IsRequired();

        builder.HasOne(u => u.EntryBy)
            .WithMany()
            .HasForeignKey("EntryByUserId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.CompanyId, u.NormalizedEmail }).IsUnique();

        // Navigation properties
        builder.Navigation(u => u.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserModulePermissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserStatuses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

---

### Step 8: Register Dependencies

#### 8.1 Dependency Injection Setup
```csharp
// In Startup.cs or Program.cs

services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR - Register CreateUserCommandHandler
services.AddMediatR(typeof(CreateUserCommandHandler).Assembly);

// AutoMapper - Auto-register profiles
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Identity
services.AddScoped<UserManager<User>>();
services.AddScoped<SignInManager<User>>();
```

---

## Database Models

### User Table Schema
```sql
CREATE TABLE users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(30),
    MiddleName NVARCHAR(30),
    LastName NVARCHAR(30),
    Email NVARCHAR(256),
    Contact NVARCHAR(256),
    PasswordHash NVARCHAR(256),
    IsEmailConfirmed BIT,
    FailedLoginAttempts INT,
    LockedUntil DATETIME2,
    CompanyId INT NOT NULL,
    EntryByUserId UNIQUEIDENTIFIER,
    EntryDate DATETIME2,
    IsSuperAdmin BIT,

    FOREIGN KEY (CompanyId) REFERENCES companies(Id),
    FOREIGN KEY (EntryByUserId) REFERENCES users(UserId),
    UNIQUE (CompanyId, Email)
);

CREATE TABLE user_roles (
    Id INT PRIMARY KEY IDENTITY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CompanyId INT,
    EntryDate DATETIME2,
    ToDate DATETIME2,

    FOREIGN KEY (UserId) REFERENCES users(UserId),
    FOREIGN KEY (RoleId) REFERENCES roles(Id)
);

CREATE TABLE user_module_permissions (
    Id INT PRIMARY KEY IDENTITY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ModulePermissionId UNIQUEIDENTIFIER NOT NULL,

    FOREIGN KEY (UserId) REFERENCES users(UserId),
    FOREIGN KEY (ModulePermissionId) REFERENCES module_permissions(Id)
);

CREATE TABLE user_statuses (
    Id INT PRIMARY KEY IDENTITY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50),
    EntryDate DATETIME2,
    ToDate DATETIME2,
    Remarks NVARCHAR(MAX),

    FOREIGN KEY (UserId) REFERENCES users(UserId)
);

CREATE TABLE agent_users (
    Id INT PRIMARY KEY IDENTITY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    AgentId INT NOT NULL,
    FromDate DATETIME2,
    ToDate DATETIME2,

    FOREIGN KEY (UserId) REFERENCES users(UserId),
    FOREIGN KEY (AgentId) REFERENCES agents(Id)
);
```

---

## Event System

### Domain Event Publishing Flow
```csharp
// 1. Event is created
var evt = new UserCreatedEvent(
    user.FullName!,
    user.UserName,
    user.Email!,
    resetPasswordUrl
);

// 2. Event is published via MediatR
await _mediator.Publish(evt);

// 3. Event handler processes it
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, 
        CancellationToken cancellationToken)
    {
        // Send welcome email
        // Log event
        // Perform other side effects
    }
}
```

---

## Error Handling

### Error Types
```csharp
public static class Errors
{
    public static ErrorDTO RoleIsRequired => 
        new("ROLE_REQUIRED", "At least one role must be assigned");

    public static ErrorDTO AgentNotFoundForBranch => 
        new("AGENT_NOT_FOUND", "Agent not found for the specified branch");

    public static ErrorDTO Exception(Exception ex) => 
        new("EXCEPTION", ex.Message);

    public static ErrorDTO Custom(string message) => 
        new("CUSTOM_ERROR", message);
}
```

### Response Structure
```csharp
{
    "success": true/false,
    "message": "Operation message",
    "errors": [
        {
            "code": "ERROR_CODE",
            "message": "Error message"
        }
    ],
    "data": { /* UserViewModel */ }
}
```

---

## Configuration

### appsettings.json
```json
{
  "MailConfig": {
    "OfficeURL": "https://yourapp.com/",
    "OfficeNewUserUrl": "auth/reset-password?token={0}&email={1}"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoreSaas;Trusted_Connection=true;"
  }
}
```

### Startup Configuration
```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMediatR(typeof(CreateUserCommandHandler).Assembly);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Identity
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/users/create", async (CreateUserCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return Results.Ok(result);
});

app.Run();
```

---

## API Endpoint

### Create User Endpoint
```http
POST /api/users/create
Content-Type: application/json
Authorization: Bearer {token}

{
    "userName": "john.doe",
    "firstName": "John",
    "middleName": "Michael",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "contact": "1234567890",
    "roles": ["role-id-1", "role-id-2"],
    "modulePermissions": ["permission-id-1", "permission-id-2"]
}
```

### Success Response
```json
{
    "success": true,
    "message": null,
    "errors": [],
    "data": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "userName": "john.doe",
        "firstName": "John",
        "middleName": "Michael",
        "lastName": "Doe",
        "fullName": "John Michael Doe",
        "email": "john.doe@example.com",
        "contact": "1234567890",
        "agentId": "agent-guid",
        "agentName": "Main Agent",
        "emailConfirmed": false,
        "roleList": [
            {
                "id": "role-guid",
                "name": "Admin",
                "desc": "Administrator Role"
            }
        ],
        "userModulePermissionList": []
    }
}
```

### Error Response
```json
{
    "success": false,
    "message": null,
    "errors": [
        {
            "code": "ROLE_REQUIRED",
            "message": "At least one role must be assigned"
        }
    ],
    "data": null
}
```

---

## Key Patterns Used

### 1. **CQRS Pattern**
- Commands: `CreateUserCommand`
- Handlers: `CreateUserCommandHandler`
- Queries: Could be added for read operations

### 2. **Repository Pattern**
- `IUnitOfWork` abstracts data access
- Generic repository for entities
- Supports querying, adding, updating, deleting

### 3. **Dependency Injection**
- Constructor injection for services
- Interface-based dependencies
- Scope management

### 4. **AutoMapper**
- Entity to DTO mapping
- Nested mappings for related entities
- Profile-based configuration

### 5. **Domain-Driven Design**
- Rich domain entities with business logic
- Value objects
- Aggregate roots (User)
- Domain events

### 6. **Event-Driven Architecture**
- Domain events published after entity changes
- Event handlers for side effects
- Eventual consistency

---

## Testing Considerations

```csharp
// Unit Test Example
[Fact]
public void User_CreateUser_WithValidData_ShouldSucceed()
{
    // Arrange
    var company = new Company("Test Co", "test@example.com", "123 Main St", 
        "555-1234", "PAN123", "REG456", "http://test.com");
    var entryUser = new User(company, "admin", "Admin", null, "User", 
        "admin@example.com", null, null);

    // Act
    var user = new User(company, "john.doe", "John", "Michael", "Doe",
        "john@example.com", "555-5678", entryUser);

    // Assert
    Assert.NotNull(user);
    Assert.Equal("John", user.FirstName);
    Assert.Equal("john@example.com", user.Email);
}
```

---

## Summary

The **CreateUser** feature demonstrates a complete implementation of:
- Clean Architecture principles
- Domain-Driven Design
- CQRS with MediatR
- Event-Driven Architecture
- Dependency Injection
- Repository Pattern
- Proper error handling
- Tenant isolation (Multi-tenancy)

This implementation ensures maintainability, testability, and scalability for the CoreSaas platform.
