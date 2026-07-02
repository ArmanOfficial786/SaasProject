# HRM SaaS - Quick Reference & Cheat Sheet

## Quick Start (5 Minutes)

### 1. Start the Application
```bash
cd D:\ARMAN\SaasProject
dotnet run --project Src/Product/Hrm.Api
```
**Output**: `Now listening on: https://localhost:7000`

### 2. Access Swagger UI
```
https://localhost:7000/swagger
```

### 3. Check Database
```bash
# SQL Server Management Studio
# Server: . (or your SQL Server name)
# Database: HrmDb
# Look for tables: user, role, permission, agent, company, etc.
```

---

## Architecture at a Glance

```
Hrm.Api (Controllers)
    ↓
UserManagement.Application (Services)
    ↓
UserManagement.Domain (Entities)
    ↓
Shared.Infrastructure (DbContext, Repositories)
    ↓
SQL Server Database
```

### Key Components

| Component | Purpose | Location |
|-----------|---------|----------|
| **User** | User account | `UserManagement.Domain/Entities/User.cs` |
| **Role** | Role definition | `UserManagement.Domain/Entities/Role.cs` |
| **Permission** | System permission | `UserManagement.Domain/Entities/Permission.cs` |
| **Agent** | Business location | `UserManagement.Domain/Entities/Agent.cs` |
| **Company** | Tenant company | `UserManagement.Domain/Entities/Company.cs` |
| **HrmDbContext** | Database access | `Shared.Infrastructure/Data/HrmDbContext/HrmDbContext.cs` |
| **TenantResolutionMiddleware** | Extract CompanyId | `Shared.Infrastructure/TenantResolutionMiddleware.cs` |

---

## Multi-Tenant Isolation (Critical!)

### How It Works

1. **JWT Token** contains `CompanyId` claim
2. **Middleware** extracts CompanyId → sets `ITenantContext`
3. **DbContext** applies `WHERE CompanyId = {current}` to all queries
4. **Services** stamp `CompanyId` on new entities

### Example: Creating a User

```csharp
// Service
var user = new User(email, name, contact, passwordHash, entryBy, entryDate);
// Automatically set: user.CompanyId = _tenantContext.CompanyId

// SaveChanges automatically filters by CompanyId
await _unitOfWork.CommitAsync();
```

### Example: Querying Users

```csharp
// Service - automatically filtered by CompanyId
var users = await _repository.GetAllAsync();
// Generates SQL: SELECT * FROM [user] WHERE CompanyId = @p0

// Only current tenant's users returned!
```

### Critical Security Rules

- ✅ **ALWAYS** filter by CompanyId in services
- ✅ **NEVER** trust client-provided CompanyId
- ✅ **ALWAYS** verify tenant context is set
- ✅ **NEVER** skip tenant filtering
- ✅ **ALWAYS** use `[Authorize]` on protected endpoints

---

## Common Code Patterns

### 1. Create Entity

```csharp
// In service method
var entity = new Entity(required, parameters);
// CompanyId automatically set by middleware/DbContext

_repository.Add(entity);
await _unitOfWork.CommitAsync();
```

### 2. Query Entity

```csharp
// Service method
var entity = await _repository.FirstOrDefaultAsync(
    e => e.Id == id && e.CompanyId == _tenantContext.CompanyId
);

// Or simply - CompanyId filter applied automatically
var entity = await _repository.GetByIdAsync(id);
```

### 3. Update Entity

```csharp
var entity = await _repository.GetByIdAsync(id);
if (entity == null) throw new NotFoundException();

// Modify properties
entity.Property = newValue;

_repository.Update(entity);
await _unitOfWork.CommitAsync();
```

### 4. Delete Entity (Soft)

```csharp
var entity = await _repository.GetByIdAsync(id);
if (entity == null) throw new NotFoundException();

entity.ToDate = DateTime.UtcNow;  // Mark as deleted

_repository.Update(entity);
await _unitOfWork.CommitAsync();
```

### 5. API Endpoint

```csharp
[Authorize]
[HttpGet("{id}")]
public async Task<IActionResult> GetEntity(Guid id)
{
    try
    {
        var entity = await _service.GetEntityAsync(id);
        return Ok(new { success = true, data = entity });
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { success = false, message = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = "Internal error" });
    }
}
```

---

## Database Schema Quick Reference

### Key Entities

```
User
├── Id (Guid) - PK
├── Email (string) - Unique per company
├── FullName (string)
├── PasswordHash (string)
├── CompanyId (Guid) - FK, Tenant isolation
├── FailedLoginAttempts (int)
├── LockedUntil (DateTime?)
├── EntryDate (DateTime)
├── EntryBy (Guid?) - FK to User
└── UserRoles (collection)

Role
├── Id (Guid) - PK
├── Name (string) - Unique per company
├── Description (string)
├── CompanyId (Guid) - FK, Tenant isolation
└── RoleModulePermissions (collection)

Permission
├── Id (Guid) - PK
├── Code (string) - e.g., "USER.CREATE"
├── Name (string)
└── RoleModulePermissions (collection)

Agent
├── Id (Guid) - PK
├── Name (string)
├── Address (string)
├── CompanyId (Guid) - FK, Tenant isolation
├── ToDate (DateTime?) - Soft delete
└── AgentUsers (collection)

Company
├── Id (int) - PK
├── Name (string) - Unique
├── EntryDate (DateTime)
└── (Contains all tenant data)
```

### Query Examples

```sql
-- Get all users in current company
SELECT * FROM [user] WHERE CompanyId = @companyId

-- Get active agents (not soft deleted)
SELECT * FROM agent WHERE CompanyId = @companyId AND ToDate IS NULL

-- Get user roles
SELECT r.* FROM role r
INNER JOIN user_role ur ON r.Id = ur.RoleId
WHERE ur.UserId = @userId AND r.CompanyId = @companyId

-- Get role permissions
SELECT p.* FROM permission p
INNER JOIN role_module_permission rmp ON p.Id = rmp.PermissionId
WHERE rmp.RoleId = @roleId
```

---

## Common Tasks & Solutions

### Task 1: Add New Entity

**Files to Create/Modify**:
1. Entity: `UserManagement.Domain/Entities/NewEntity.cs`
2. Configuration: `Shared.Infrastructure/Data/Configurations/NewEntityConfiguration.cs`
3. DbSet: Add to `HrmDbContext.cs`
4. Repository: Already generic, no changes needed

**Steps**:
```csharp
// 1. Create entity
public class NewEntity : AuditableEntity
{
    public string Name { get; set; }
    public Guid CompanyId { get; set; }  // Important!
}

// 2. Create configuration
public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        builder.ToTable("new_entity");
        builder.Property(x => x.CompanyId).IsRequired();
        builder.HasIndex(x => x.CompanyId);
    }
}

// 3. Add DbSet to HrmDbContext
public DbSet<NewEntity> NewEntities => Set<NewEntity>();

// 4. Run migration
dotnet ef migrations add AddNewEntity --startup-project ../../Product/Hrm.Api
dotnet ef database update --startup-project ../../Product/Hrm.Api
```

### Task 2: Add New API Endpoint

**Files to Create**:
1. DTO: `UserManagement.Application/DTOs/NewDtos.cs`
2. Service: `UserManagement.Application/Services/NewService.cs`
3. Controller: `Product/Hrm.Api/Controllers/NewController.cs`

**Steps**:
```csharp
// 1. Create DTOs
public class GetNewResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class CreateNewRequest
{
    [Required]
    public string Name { get; set; }
}

// 2. Create service interface
public interface INewService
{
    Task<GetNewResponse> GetAsync(Guid id);
    Task<GetNewResponse> CreateAsync(CreateNewRequest request);
}

// 3. Create service implementation
public class NewService : INewService
{
    public async Task<GetNewResponse> GetAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new NotFoundException();
        return _mapper.Map<GetNewResponse>(entity);
    }

    public async Task<GetNewResponse> CreateAsync(CreateNewRequest request)
    {
        var entity = new NewEntity { Name = request.Name };
        _repository.Add(entity);
        await _unitOfWork.CommitAsync();
        return _mapper.Map<GetNewResponse>(entity);
    }
}

// 4. Create controller
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewController : ControllerBase
{
    private readonly INewService _service;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateNewRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, 
            new { success = true, data = result });
    }
}

// 5. Register in DI (Program.cs)
builder.Services.AddScoped<INewService, NewService>();
```

### Task 3: Add Validation

**Using Data Annotations**:
```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256)]
    public string Email { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; }
}
```

**Using FluentValidation** (if installed):
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}
```

---

## Debugging Tips

### 1. Check Database
```bash
# Open SQL Server Management Studio
# Connect to: . (or your SQL Server)
# Database: HrmDb
# Query:
SELECT * FROM [user]
SELECT * FROM [user] WHERE CompanyId = 'your-company-guid'
```

### 2. Enable Detailed Logging
```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Or in appsettings.json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.EntityFrameworkCore": "Debug"
  }
}
```

### 3. SQL Query Debugging
```csharp
// Add to HrmDbContext constructor
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.LogTo(Console.WriteLine);
}

// Now see all generated SQL queries in console
```

### 4. Token Debugging
```csharp
// Decode JWT at https://jwt.io/
// Check claims include: UserId, CompanyId, Email, Roles
// Verify expiration time

// Or decode in code:
var handler = new JwtSecurityTokenHandler();
var token = handler.ReadJwtToken(tokenString);
var claims = token.Claims;
foreach (var claim in claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}
```

### 5. API Testing
```bash
# Use curl or Postman
# Check response headers, status code, body
# Verify error messages are helpful

# Example with verbose output
curl -v -X GET "https://localhost:7000/api/user" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k
```

---

## Performance Optimization Checklist

- [ ] Database indexes on CompanyId
- [ ] Database indexes on frequently queried fields
- [ ] Pagination on list endpoints (50 items default)
- [ ] Eager loading (Include) for related data
- [ ] Async/await throughout (no .Result or .Wait())
- [ ] Connection pooling configured
- [ ] Response compression enabled
- [ ] Caching for frequently accessed data

### Example: Add Index

```csharp
// In EntityConfiguration
builder.HasIndex(x => x.Email)
    .IsUnique();

builder.HasIndex(x => new { x.CompanyId, x.Email })
    .IsUnique();

builder.HasIndex(x => x.CompanyId);
```

---

## Git Commands Quick Reference

```bash
# Clone repository
git clone https://github.com/ArmanOfficial786/CoreSaas.git

# Check status
git status

# Add changes
git add .

# Commit
git commit -m "Phase 1: Implement authentication"

# Push
git push origin main

# Pull latest
git pull origin main

# Create branch
git checkout -b feature/user-management

# Switch branch
git checkout main

# View history
git log --oneline
```

---

## Useful Commands

```bash
# Build solution
dotnet build

# Run project
dotnet run --project Src/Product/Hrm.Api

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName \
  --startup-project Src/Product/Hrm.Api \
  --project Src/Shared/Shared.Infrastructure

# Update database
dotnet ef database update \
  --startup-project Src/Product/Hrm.Api

# Remove last migration
dotnet ef migrations remove

# Format code (if using dotnet format)
dotnet format

# List all projects
dotnet sln Src list

# Clean build artifacts
dotnet clean
```

---

## File Navigation Map

```
D:\ARMAN\SaasProject\
│
├── Src\Product\Hrm.Api\
│   ├── Program.cs              ← Startup configuration
│   ├── appsettings.json        ← Connection string
│   ├── Controllers\            ← Create new controllers here
│   ├── Properties\launchSettings.json ← Port configuration
│   └── SeedData\               ← Initial data files
│
├── Src\UserManagement\UserManagement.Domain\Entities\
│   ├── User.cs                 ← User entity
│   ├── Role.cs                 ← Role entity
│   ├── Permission.cs           ← Permission entity
│   ├── Agent.cs                ← Agent entity
│   └── Company.cs              ← Company entity
│
├── Src\UserManagement\UserManagement.Application\
│   ├── Services\               ← Create services here
│   ├── DTOs\                   ← Create DTOs here
│   └── ViewModels\             ← Create ViewModels here
│
└── Src\Shared\Shared.Infrastructure\Data\
    ├── HrmDbContext\           ← DbContext
    └── Configurations\         ← Entity configurations
```

---

## Important URLs & Ports

| Service | URL | Port | Notes |
|---------|-----|------|-------|
| **API** | https://localhost:7000 | 7000 | HTTPS default |
| **API (HTTP)** | http://localhost:5000 | 5000 | HTTP fallback |
| **Swagger UI** | https://localhost:7000/swagger | - | API documentation |
| **SQL Server** | . (localhost) | 1433 | Database server |
| **Management Studio** | - | - | For database admin |

---

## Common Error Messages & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| "Connection string not found" | Missing appsettings.json | Check connection string in appsettings.json |
| "DbSet not found" | Entity not registered | Add DbSet<Entity> to HrmDbContext |
| "DbContext not found" | Missing DI registration | Add AddHrmDbContext in Program.cs |
| "401 Unauthorized" | Missing/invalid token | Include Authorization header with valid JWT |
| "403 Forbidden" | Insufficient permissions | Verify user has required role |
| "Entity not found" | Invalid ID or wrong CompanyId | Verify entity exists in current tenant |
| "Duplicate entry" | Unique constraint violation | Email/name already exists in company |
| "Migration failed" | Database schema mismatch | Run dotnet ef migrations add and update |

---

## Best Practices Checklist

Code Quality:
- [ ] Follow naming conventions (PascalCase for classes, camelCase for fields)
- [ ] Use meaningful variable names
- [ ] Add XML documentation to public methods
- [ ] Keep methods small and focused
- [ ] Use dependency injection
- [ ] Implement error handling

Database:
- [ ] Always filter by CompanyId
- [ ] Use pagination for lists
- [ ] Create migrations for schema changes
- [ ] Test migrations before deploying
- [ ] Backup database regularly

Security:
- [ ] Use [Authorize] attributes
- [ ] Hash passwords securely
- [ ] Validate all input
- [ ] Never trust client-provided CompanyId
- [ ] Use HTTPS only in production
- [ ] Rotate JWT secrets regularly

Testing:
- [ ] Write unit tests for services
- [ ] Write integration tests for APIs
- [ ] Test error scenarios
- [ ] Test multi-tenant isolation
- [ ] Test authorization scenarios

---

## Ready to Start?

1. **Verify**: `dotnet build` ✅
2. **Database**: Run migrations ✅
3. **Run**: `dotnet run --project Src/Product/Hrm.Api` ✅
4. **Test**: Open Swagger at `https://localhost:7000/swagger` ✅
5. **Code**: Start Phase 1 implementation! 🚀

---

**Quick Links to Documentation**:
- 📖 **PROJECT_SETUP_FROM_BEGINNING.md** - Complete setup guide
- 🎯 **NEXT_STEPS_ACTION_PLAN.md** - Week-by-week plan
- 📋 **API_SETUP_GUIDE.md** - Architecture & design
- 💻 **API_CODE_SNIPPETS.md** - Ready-to-use code
- ✅ **API_IMPLEMENTATION_CHECKLIST.md** - Task checklist

---

**Last Updated**: December 2024  
**Status**: Ready to Use  
**Version**: 1.0
