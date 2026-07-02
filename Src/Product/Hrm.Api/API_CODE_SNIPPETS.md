# API Implementation Code Snippets from here

This file contains ready-to-use code snippets for implementing the HRM API.

---

## 1. Authentication Setup

### 1.1 Token Service Implementation

**File**: `Src/UserManagement/UserManagement.Application/Services/TokenService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Application.Services;

public class TokenClaims
{
    public string UserId { get; set; }
    public string CompanyId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public interface ITokenService
{
    string GenerateToken(TokenClaims claims);
    ClaimsPrincipal ValidateToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(TokenClaims claims)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new Claim("UserId", claims.UserId),
            new Claim("CompanyId", claims.CompanyId),
            new Claim("Email", claims.Email),
            new Claim("FullName", claims.FullName),
        };

        // Add roles
        foreach (var role in claims.Roles)
        {
            tokenClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions
        foreach (var permission in claims.Permissions)
        {
            tokenClaims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: tokenClaims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

### 1.2 Authentication Service

**File**: `Src/UserManagement/UserManagement.Application/Services/AuthenticationService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Application.Services;

public class AuthResponse
{
    public string Token { get; set; }
    public UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateCredentialsAsync(string email, string password);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            throw new UnauthorizedException("Account is locked. Please try again later.");

        var roles = user.UserRoles
            .Where(ur => ur.ToDate == null)
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = await GetUserPermissionsAsync(user.Id);

        var tokenClaims = new TokenClaims
        {
            UserId = user.Id.ToString(),
            CompanyId = user.CompanyId.ToString(),
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            Permissions = permissions
        };

        var token = _tokenService.GenerateToken(tokenClaims);

        // Log successful login
        _unitOfWork.Repository<LoginLog>().Add(new LoginLog
        {
            UserId = user.Id,
            LoginTime = DateTime.UtcNow,
            IsSuccessful = true,
            CompanyId = user.CompanyId
        });

        user.FailedLoginAttempts = 0;
        await _unitOfWork.CommitAsync();

        return new AuthResponse
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == email);

        return user != null && VerifyPassword(password, user.PasswordHash);
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new List<string>();

        var permissions = user.UserRoles
            .Where(ur => ur.ToDate == null)
            .SelectMany(ur => ur.Role.RoleModulePermissions)
            .Select(rmp => $"{rmp.ModulePermission.Module.Code}.{rmp.ModulePermission.Code}")
            .Distinct()
            .ToList();

        return permissions;
    }

    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(saltBytes, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }
    }

    public static bool VerifyPassword(string password, string hash)
    {
        byte[] hashBytes = Convert.FromBase64String(hash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(20);

        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != computedHash[i])
                return false;
        }

        return true;
    }
}
```

### 1.3 Program.cs Configuration

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hrm.Api.Extensions;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HRM DbContext
var hrmConnectionString = builder.Configuration.GetConnectionString("HrmConnection")
    ?? throw new InvalidOperationException("Connection string 'HrmConnection' not found.");
builder.Services.AddHrmDbContext(hrmConnectionString);

// Add Shared Infrastructure
builder.Services.AddSharedInfrastructure();

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
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAgentService, AgentService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.MapControllers();

// Seed database
try
{
    await app.InitialiseDatabaseAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Database seeding encountered an error.");
}

app.Run();
```

---

## 2. Controller Examples

### 2.1 User Controller

**File**: `Src/Product/Hrm.Api/Controllers/UserController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UserManagement.Application.Services;

namespace Hrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _userService.LoginAsync(request);
            return Ok(new { success = true, data = response });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var profile = await _userService.GetCurrentUserProfileAsync();
            return Ok(new { success = true, data = profile });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var users = await _userService.GetUsersAsync(pageNumber, pageSize);
            return Ok(new { success = true, data = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(new { success = true, data = user });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                new { success = true, data = user });
        }
        catch (DuplicateException ex)
        {
            return Conflict(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(new { success = true, data = user });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return Forbid();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            return result ? Ok(new { success = true }) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}
```

### 2.2 Agent Controller

**File**: `Src/Product/Hrm.Api/Controllers/AgentController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UserManagement.Application.Services;

namespace Hrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var agents = await _agentService.GetAgentsAsync(pageNumber, pageSize);
            return Ok(new { success = true, data = agents });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgent(Guid id)
    {
        try
        {
            var agent = await _agentService.GetAgentAsync(id);
            return Ok(new { success = true, data = agent });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var agent = await _agentService.CreateAgentAsync(request);
            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, 
                new { success = true, data = agent });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAgent(Guid id, [FromBody] UpdateAgentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var agent = await _agentService.UpdateAgentAsync(id, request);
            return Ok(new { success = true, data = agent });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(Guid id)
    {
        try
        {
            var result = await _agentService.DeleteAgentAsync(id);
            return result ? Ok(new { success = true }) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}
```

### 2.3 Company Controller

**File**: `Src/Product/Hrm.Api/Controllers/CompanyController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UserManagement.Application.Services;

namespace Hrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        try
        {
            var company = await _companyService.GetCompanyAsync(id);
            return Ok(new { success = true, data = company });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var company = await _companyService.CreateCompanyAsync(request);
            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, 
                new { success = true, data = company });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var company = await _companyService.UpdateCompanyAsync(id, request);
            return Ok(new { success = true, data = company });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
}
```

---

## 3. Service Implementation Examples

### 3.1 User Service

**File**: `Src/UserManagement/UserManagement.Application/Services/UserService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

public interface IUserService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetCurrentUserProfileAsync();
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> GetUserAsync(Guid id);
    Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IUnitOfWork unitOfWork,
        IAuthenticationService authenticationService,
        ITenantContext tenantContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
        _tenantContext = tenantContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        return await _authenticationService.LoginAsync(request);
    }

    public async Task<UserDto> GetCurrentUserProfileAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("User ID not found in claims");

        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == _tenantContext.CompanyId);

        if (user == null)
            throw new NotFoundException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Validate email uniqueness per company
        var existingUser = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.CompanyId == _tenantContext.CompanyId);

        if (existingUser != null)
            throw new DuplicateException("User with this email already exists in the company");

        var passwordHash = AuthenticationService.HashPassword(request.Password);

        var user = new User(
            request.Email,
            request.FullName,
            request.Contact,
            passwordHash,
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
                var role = await _unitOfWork.Repository<Role>()
                    .FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == _tenantContext.CompanyId);

                if (role != null)
                {
                    user.AddRole(role);
                }
            }
        }

        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserAsync(Guid id)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _tenantContext.CompanyId);

        if (user == null)
            throw new NotFoundException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var pagedUsers = await _unitOfWork.Repository<User>()
            .GetPagedAsync(
                filter: u => u.CompanyId == _tenantContext.CompanyId,
                orderBy: q => q.OrderByDescending(u => u.EntryDate),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

        return new PagedResult<UserDto>
        {
            Items = pagedUsers.Items.Select(u => _mapper.Map<UserDto>(u)).ToList(),
            TotalCount = pagedUsers.TotalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _tenantContext.CompanyId);

        if (user == null)
            throw new NotFoundException("User not found");

        // Check if trying to update own profile or admin
        var currentUserIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        var isOwnProfile = Guid.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == id;
        var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

        if (!isOwnProfile && !isAdmin)
            throw new ForbiddenException("You don't have permission to update this user");

        if (!string.IsNullOrEmpty(request.FullName))
            user.GetType().GetProperty("FullName")?.SetValue(user, request.FullName);

        if (!string.IsNullOrEmpty(request.Contact))
            user.GetType().GetProperty("Contact")?.SetValue(user, request.Contact);

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == _tenantContext.CompanyId);

        if (user == null)
            return false;

        _unitOfWork.Repository<User>().Delete(user);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
```

---

## 4. DTO Classes

**File**: `Src/UserManagement/UserManagement.Application/DTOs/UserDtos.cs`

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Contact { get; set; }
    public Guid CompanyId { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime EntryDate { get; set; }
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

public class UpdateUserRequest
{
    public string FullName { get; set; }
    public string Contact { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

---

## 5. Exception Classes

**File**: `Src/Shared/Shared.Domain/Exceptions/ApplicationExceptions.cs`

```csharp
using System;

namespace Shared.Domain.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message) { }
}

public class DuplicateException : ApplicationException
{
    public DuplicateException(string message) : base(message) { }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message) { }
}

public class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message) { }
}
```

---

## 6. AutoMapper Profile

**File**: `Src/UserManagement/UserManagement.Application/Mappings/UserMappingProfile.cs`

```csharp
using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles
                    .Where(ur => ur.ToDate == null)
                    .Select(ur => ur.Role.Name)
                    .ToList()
            ))
            .ReverseMap();

        CreateMap<CreateUserRequest, User>().ReverseMap();
    }
}

public class AgentMappingProfile : Profile
{
    public AgentMappingProfile()
    {
        CreateMap<Agent, AgentDto>().ReverseMap();
        CreateMap<CreateAgentRequest, Agent>().ReverseMap();
    }
}

public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<CreateCompanyRequest, Company>().ReverseMap();
    }
}

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<CreateRoleRequest, Role>().ReverseMap();
    }
}
```

---

## 7. appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "HrmConnection": "Server=YOUR_SERVER;Database=HrmDb;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-minimum-32-characters-long-here",
    "Issuer": "HrmApi",
    "Audience": "HrmUsers",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

**Note**: Replace placeholder values (YOUR_SERVER, secret keys, etc.) with actual configuration values before deployment.

---

**Last Updated**: 2024  
**Version**: 1.0
