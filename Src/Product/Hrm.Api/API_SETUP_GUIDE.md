# HRM API Setup Guide - Multi-Tenant SaaS Architecture

## Overview
This guide provides a step-by-step plan for setting up and implementing the HRM API endpoints following a logical dependency order for a multi-tenant SaaS application using a single database with explicit CompanyId properties.

---

## Phase 1: Foundation & Authentication Setup

### 1.1 Database & Initialization
**Objective**: Ensure database is ready and seeded with baseline data

**Steps**:
1. Run database migrations to create all tables
2. Verify connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "HrmConnection": "Server=YOUR_SERVER;Database=YOUR_DB;..."
     }
   }
   ```
3. Verify seed data locations:
   - `SeedData/AuthData/admin.json` - Admin user credentials
   - `SeedData/AuthData/tenant.json` - Tenant/Company data
   - `SeedData/AuthData/permissions.json` - Permission definitions

4. Run database seeding via `InitialiseDatabaseAsync()` in Program.cs

**Files Involved**:
- `Src/Shared/Shared.Infrastructure/Data/HrmDbContext/HrmDbContext.cs`
- `Src/Product/Hrm.Api/Extensions/InitialiserExtensions.cs`

---

### 1.2 Authentication & JWT Setup
**Objective**: Establish JWT authentication and token generation

**Implementation Steps**:

#### A. Create Authentication Service
**File**: `Src/UserManagement/UserManagement.Application/Services/AuthenticationService.cs`

```csharp
public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Find user by email and verify password
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        // 2. Get user roles and permissions
        var userRoles = user.UserRoles.Where(ur => ur.ToDate == null);
        var permissions = await GetUserPermissionsAsync(user.Id);

        // 3. Generate JWT token with CompanyId claim
        var token = _tokenService.GenerateToken(new TokenClaims
        {
            UserId = user.Id.ToString(),
            CompanyId = user.CompanyId.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            Roles = userRoles.Select(r => r.Role.Name).ToList(),
            Permissions = permissions
        });

        return new AuthResponse { Token = token };
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        // Get permissions based on user roles
        var userRoles = await _userRepository.GetUserRolesAsync(userId);
        return userRoles
            .SelectMany(r => r.Role.RoleModulePermissions)
            .Select(rmp => $"{rmp.ModulePermission.Module.Code}.{rmp.ModulePermission.Code}")
            .Distinct()
            .ToList();
    }
}
```

#### B. Create Token Service
**File**: `Src/UserManagement/UserManagement.Application/Services/TokenService.cs`

```csharp
public interface ITokenService
{
    string GenerateToken(TokenClaims claims);
    ClaimsPrincipal ValidateToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public string GenerateToken(TokenClaims claims)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new List<Claim>
            {
                new("UserId", claims.UserId),
                new("CompanyId", claims.CompanyId),
                new("Email", claims.Email),
                new("FullName", claims.FullName),
                claims.Roles.ForEach(r => new Claim(ClaimTypes.Role, r)),
                claims.Permissions.ForEach(p => new Claim("permission", p))
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### C. Register Services
**File**: `Src/UserManagement/UserManagement.Application/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddUserManagementServices(
    this IServiceCollection services)
{
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICompanyService, CompanyService>();
    services.AddScoped<IAgentService, AgentService>();

    return services;
}
```

**Configure in Program.cs**:
```csharp
// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Application Services
builder.Services.AddUserManagementServices();
```

**Update appsettings.json**:
```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-minimum-32-characters",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60
  }
}
```

---

### 1.3 Tenant Resolution Middleware
**Objective**: Extract CompanyId from JWT and set tenant context

**File**: Already exists at `Src/Shared/Shared.Infrastructure/TenantResolutionMiddleware.cs`

**Update Program.cs**:
```csharp
// Add middleware after authentication
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
```

---

## Phase 2: Core Master Data APIs

### 2.1 Company Management API
**Objective**: Create, read, update company master data (Admin only)

**Create Files**:

#### A. CompanyController
**File**: `Src/Product/Hrm.Api/Controllers/CompanyController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _companyService.GetCompanyAsync(id);
        return Ok(company);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCompany(CreateCompanyRequest request)
    {
        var company = await _companyService.CreateCompanyAsync(request);
        return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyRequest request)
    {
        var company = await _companyService.UpdateCompanyAsync(id, request);
        return Ok(company);
    }
}
```

#### B. CompanyService
**File**: `Src/UserManagement/UserManagement.Application/Services/CompanyService.cs`

```csharp
public interface ICompanyService
{
    Task<CompanyDto> GetCompanyAsync(int id);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request);
    Task<CompanyDto> UpdateCompanyAsync(int id, UpdateCompanyRequest request);
    Task<bool> DeleteCompanyAsync(int id);
}

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request)
    {
        var company = new Company(
            request.Name,
            request.Email,
            request.Address,
            request.PhoneNo,
            request.Pan,
            request.RegNo,
            request.Url
        );

        _unitOfWork.Repository<Company>().Add(company);
        await _unitOfWork.CommitAsync();

        return MapToDto(company);
    }
}
```

**DTOs**:
```csharp
public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Pan { get; set; }
    public string RegNo { get; set; }
}

public class CreateCompanyRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    public string Address { get; set; }
    public string PhoneNo { get; set; }
    public string Pan { get; set; }
    public string RegNo { get; set; }
    public string Url { get; set; }
}
```

**Dependency**: User authentication + Database initialized

---

### 2.2 Agent Management API
**Objective**: Create and manage agents within a company

**File**: `Src/Product/Hrm.Api/Controllers/AgentController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    [HttpGet]
    public async Task<IActionResult> GetAgents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var agents = await _agentService.GetAgentsAsync(pageNumber, pageSize);
        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgent(Guid id)
    {
        var agent = await _agentService.GetAgentAsync(id);
        return Ok(agent);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateAgent(CreateAgentRequest request)
    {
        var agent = await _agentService.CreateAgentAsync(request);
        return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agent);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateAgent(Guid id, UpdateAgentRequest request)
    {
        var agent = await _agentService.UpdateAgentAsync(id, request);
        return Ok(agent);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAgent(Guid id)
    {
        var result = await _agentService.DeleteAgentAsync(id);
        return result ? Ok() : NotFound();
    }
}
```

**Service with CompanyId filtering**:
```csharp
public class AgentService : IAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public async Task<PagedResult<AgentDto>> GetAgentsAsync(int pageNumber, int pageSize)
    {
        var agents = await _unitOfWork.Repository<Agent>()
            .GetPagedAsync(
                filter: a => a.CompanyId == _tenantContext.CompanyId,
                pageNumber: pageNumber,
                pageSize: pageSize
            );

        return new PagedResult<AgentDto>
        {
            Items = agents.Items.Select(MapToDto).ToList(),
            TotalCount = agents.TotalCount
        };
    }

    public async Task<AgentDto> CreateAgentAsync(CreateAgentRequest request)
    {
        var agent = new Agent(
            request.Name,
            request.Address,
            request.Pan,
            request.RegNo,
            request.IsParent,
            request.ReferralCode,
            null,
            GetCurrentCompany(),
            _tenantContext.CompanyId
        );

        _unitOfWork.Repository<Agent>().Add(agent);
        await _unitOfWork.CommitAsync();

        return MapToDto(agent);
    }
}
```

**Dependency**: Company exists, User authenticated with CompanyId claim

---

### 2.3 Agent Role Assignment API
**Objective**: Assign roles to agents

**File**: `Src/Product/Hrm.Api/Controllers/AgentRoleController.cs`

```csharp
[ApiController]
[Route("api/agents/{agentId}/[controller]")]
[Authorize]
public class AgentRoleController : ControllerBase
{
    private readonly IAgentRoleService _agentRoleService;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoleToAgent(Guid agentId, AssignAgentRoleRequest request)
    {
        var result = await _agentRoleService.AssignRoleAsync(agentId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAgentRoles(Guid agentId)
    {
        var roles = await _agentRoleService.GetAgentRolesAsync(agentId);
        return Ok(roles);
    }

    [HttpDelete("{roleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRoleFromAgent(Guid agentId, Guid roleId)
    {
        var result = await _agentRoleService.RemoveRoleAsync(agentId, roleId);
        return result ? Ok() : NotFound();
    }
}
```

**Dependency**: Agent exists

---

## Phase 3: User Management APIs

### 3.1 User Management API
**Objective**: Create and manage users with role assignment

**File**: `Src/Product/Hrm.Api/Controllers/UserController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _userService.LoginAsync(request);
        return Ok(response);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _userService.GetCurrentUserProfileAsync();
        return Ok(profile);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetProfile), user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _userService.GetUsersAsync(pageNumber, pageSize);
        return Ok(users);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result ? Ok() : NotFound();
    }
}
```

**UserService Implementation**:
```csharp
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        return await _authenticationService.LoginAsync(request);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Validate email uniqueness per company
        var existingUser = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.CompanyId == _tenantContext.CompanyId);

        if (existingUser != null)
            throw new DuplicateException("User with this email already exists");

        var user = new User(
            request.Email,
            request.FullName,
            request.Contact,
            HashPassword(request.Password),
            null,
            DateTime.UtcNow
        )
        {
            CompanyId = _tenantContext.CompanyId
        };

        _unitOfWork.Repository<User>().Add(user);

        // Add default roles if specified
        if (request.RoleIds?.Any() == true)
        {
            foreach (var roleId in request.RoleIds)
            {
                user.AddRole(new Role(/* from repo */));
            }
        }

        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _unitOfWork.Repository<User>()
            .GetPagedAsync(
                filter: u => u.CompanyId == _tenantContext.CompanyId,
                pageNumber: pageNumber,
                pageSize: pageSize
            );

        return new PagedResult<UserDto>
        {
            Items = users.Items.Select(u => _mapper.Map<UserDto>(u)).ToList(),
            TotalCount = users.TotalCount
        };
    }
}
```

**DTOs**:
```csharp
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(3)]
    public string FullName { get; set; }

    public string Contact { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    public List<Guid> RoleIds { get; set; } = new();
}

public class AuthResponse
{
    public string Token { get; set; }
    public UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

**Dependency**: Company exists, Agent system initialized

---

### 3.2 User Role Assignment API
**Objective**: Assign and manage roles for users

**File**: `Src/Product/Hrm.Api/Controllers/UserRoleController.cs`

```csharp
[ApiController]
[Route("api/users/{userId}/roles")]
[Authorize]
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    [HttpGet]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var roles = await _userRoleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, AssignRoleRequest request)
    {
        var result = await _userRoleService.AssignRoleAsync(userId, request);
        return Ok(result);
    }

    [HttpDelete("{roleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        var result = await _userRoleService.RemoveRoleAsync(userId, roleId);
        return result ? Ok() : NotFound();
    }
}
```

**Dependency**: User exists, Role system initialized

---

## Phase 4: Role & Permission Management APIs

### 4.1 Role Management API
**Objective**: Create and manage roles per company

**File**: `Src/Product/Hrm.Api/Controllers/RoleController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        var role = await _roleService.GetRoleAsync(id);
        return Ok(role);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request)
    {
        var role = await _roleService.CreateRoleAsync(request);
        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleRequest request)
    {
        var role = await _roleService.UpdateRoleAsync(id, request);
        return Ok(role);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        return result ? Ok() : NotFound();
    }
}
```

**RoleService**:
```csharp
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        var role = new Role(request.Name, request.Description)
        {
            CompanyId = _tenantContext.CompanyId
        };

        _unitOfWork.Repository<Role>().Add(role);
        await _unitOfWork.CommitAsync();

        return MapToDto(role);
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _unitOfWork.Repository<Role>()
            .FindAsync(r => r.CompanyId == _tenantContext.CompanyId && r.ToDate == null);

        return roles.Select(MapToDto).ToList();
    }
}
```

**DTOs**:
```csharp
public class CreateRoleRequest
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
```

**Dependency**: Company exists, User authenticated

---

### 4.2 Permission Management API
**Objective**: Manage permissions and their relationships to modules

**File**: `Src/Product/Hrm.Api/Controllers/PermissionController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _permissionService.GetPermissionsAsync();
        return Ok(permissions);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePermission(CreatePermissionRequest request)
    {
        var permission = await _permissionService.CreatePermissionAsync(request);
        return CreatedAtAction(nameof(GetPermissions), permission);
    }
}
```

**Dependency**: Module system initialized

---

### 4.3 Role Permission Assignment API
**Objective**: Assign permissions to roles

**File**: `Src/Product/Hrm.Api/Controllers/RolePermissionController.cs`

```csharp
[ApiController]
[Route("api/roles/{roleId}/permissions")]
[Authorize]
public class RolePermissionController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignPermissionToRole(Guid roleId, AssignPermissionRequest request)
    {
        var result = await _rolePermissionService.AssignPermissionAsync(roleId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        var permissions = await _rolePermissionService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }

    [HttpDelete("{permissionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        var result = await _rolePermissionService.RemovePermissionAsync(roleId, permissionId);
        return result ? Ok() : NotFound();
    }
}
```

**Dependency**: Role exists, Permission exists

---

## Phase 5: Advanced Features

### 5.1 Module Management API
**File**: `Src/Product/Hrm.Api/Controllers/ModuleController.cs`

**Objective**: List available modules and their permissions

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModuleController : ControllerBase
{
    private readonly IModuleService _moduleService;

    [HttpGet]
    public async Task<IActionResult> GetModules()
    {
        var modules = await _moduleService.GetAvailableModulesAsync();
        return Ok(modules);
    }

    [HttpGet("{moduleId}/permissions")]
    public async Task<IActionResult> GetModulePermissions(Guid moduleId)
    {
        var permissions = await _moduleService.GetModulePermissionsAsync(moduleId);
        return Ok(permissions);
    }
}
```

---

### 5.2 User Permissions API
**File**: `Src/Product/Hrm.Api/Controllers/UserPermissionController.cs`

**Objective**: Get effective permissions for current user

```csharp
[ApiController]
[Route("api/users/me/permissions")]
[Authorize]
public class UserPermissionController : ControllerBase
{
    private readonly IUserPermissionService _userPermissionService;

    [HttpGet]
    public async Task<IActionResult> GetMyPermissions()
    {
        var permissions = await _userPermissionService.GetCurrentUserPermissionsAsync();
        return Ok(permissions);
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckPermission(CheckPermissionRequest request)
    {
        var hasPermission = await _userPermissionService.HasPermissionAsync(request.Permission);
        return Ok(new { hasPermission });
    }
}
```

---

### 5.3 Agent User Assignment API
**File**: `Src/Product/Hrm.Api/Controllers/AgentUserController.cs`

**Objective**: Assign users to agents

```csharp
[ApiController]
[Route("api/agents/{agentId}/users")]
[Authorize]
public class AgentUserController : ControllerBase
{
    private readonly IAgentUserService _agentUserService;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignUserToAgent(Guid agentId, AssignUserRequest request)
    {
        var result = await _agentUserService.AssignUserAsync(agentId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAgentUsers(Guid agentId)
    {
        var users = await _agentUserService.GetAgentUsersAsync(agentId);
        return Ok(users);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveUserFromAgent(Guid agentId, Guid userId)
    {
        var result = await _agentUserService.RemoveUserAsync(agentId, userId);
        return result ? Ok() : NotFound();
    }
}
```

---

## Implementation Order

### **Week 1: Foundation**
1. ✅ Database initialization & seeding
2. ✅ JWT authentication setup
3. ✅ Tenant context resolution
4. ✅ Base service layer architecture

### **Week 2: Core Master Data**
1. ✅ Company API (CRUD)
2. ✅ Agent API (CRUD with CompanyId filtering)
3. ✅ Agent Role assignment API

### **Week 3: User Management**
1. ✅ User Login API
2. ✅ User Creation API
3. ✅ User Profile API
4. ✅ User List API (with pagination)
5. ✅ User Role assignment API

### **Week 4: Permissions & Access Control**
1. ✅ Role Management API
2. ✅ Permission Management API
3. ✅ Role-Permission assignment API
4. ✅ User Permissions API

### **Week 5: Integration & Polish**
1. ✅ Agent-User assignment API
2. ✅ Module API
3. ✅ Error handling & validation
4. ✅ API documentation
5. ✅ Performance optimization

---

## Key Architectural Principles

### **1. Multi-Tenant Isolation**
```csharp
// Always filter by CompanyId from tenant context
var entities = await _unitOfWork.Repository<T>()
    .FindAsync(e => e.CompanyId == _tenantContext.CompanyId);
```

### **2. Authorization Pattern**
```csharp
[Authorize(Roles = "Admin,Manager")]
[Authorize(Policy = "HasPermission:agent.view")]
public async Task<IActionResult> GetAgents()
{
    // Implementation
}
```

### **3. Error Handling**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<ValidationError> Errors { get; set; }
}
```

### **4. Pagination Pattern**
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

---

## Testing Strategy

### **1. Unit Tests**
- Service layer logic
- Validation rules
- Data mapping

### **2. Integration Tests**
- Database operations
- Multi-tenant isolation
- Permission checks

### **3. API Tests**
- Endpoint functionality
- Authorization enforcement
- Error handling

### **4. Postman Collections**
- Create collections for each API group
- Include example requests/responses
- Document authentication flow

---

## Security Checklist

- [ ] JWT tokens include CompanyId claim
- [ ] All queries filter by CompanyId
- [ ] Password hashing (bcrypt/PBKDF2)
- [ ] HTTPS enforcement
- [ ] CORS properly configured
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention
- [ ] Rate limiting configured
- [ ] Audit logging implemented
- [ ] Sensitive data not logged

---

## Database Seeding Strategy

**Initial Admin Setup**:
```json
{
  "admin_user": {
    "email": "admin@company.com",
    "password": "CHANGE_ME",
    "fullName": "System Administrator",
    "companyId": "GENERATED_GUID"
  },
  "admin_role": {
    "name": "Admin",
    "description": "Full system access"
  },
  "default_permissions": [
    "company.create",
    "company.read",
    "agent.create",
    "agent.read",
    "user.create",
    "user.read"
  ]
}
```

---

## API Documentation

### **Base URL**
```
https://yourdomain.com/api
```

### **Authentication Header**
```
Authorization: Bearer {jwt_token}
```

### **Company Header** (Alternative to JWT claim)
```
X-Company-ID: {company_id}
```

---

## Next Steps

1. **Implement Phase 1** - Authentication & JWT setup
2. **Test with Postman** - Verify each endpoint
3. **Add Swagger/OpenAPI** documentation
4. **Implement error handling** middleware
5. **Add logging** (Serilog)
6. **Set up CI/CD** pipeline
7. **Load testing** with high-concurrency scenarios
8. **Security audit** before production

---

## References

- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OWASP API Security](https://owasp.org/www-project-api-security/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Last Updated**: 2024  
**Version**: 1.0  
**Status**: Ready for Implementation
