# HRM SaaS Project Setup - Complete Guide

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Technology Stack](#technology-stack)
4. [Project Structure](#project-structure)
5. [Prerequisites & Setup](#prerequisites--setup)
6. [Database Setup](#database-setup)
7. [Running the Project](#running-the-project)
8. [API Implementation Phases](#api-implementation-phases)
9. [Next Steps](#next-steps)
10. [Troubleshooting](#troubleshooting)

---

## Project Overview

**Project Name**: HRM SaaS (Human Resource Management System)  
**Type**: Multi-tenant SaaS Platform  
**Architecture**: Clean Architecture with DDD (Domain-Driven Design)  
**Target Framework**: .NET 10  
**Database**: SQL Server  

### Key Features
- ✅ Multi-tenant support with single database (CompanyId-based isolation)
- ✅ Role-based access control (RBAC) with permissions
- ✅ JWT token authentication
- ✅ User, Agent, Company, and Agent management
- ✅ Module-based permission system
- ✅ Audit logging with EntryBy and EntryDate
- ✅ Soft delete support (ToDate field)
- ✅ Tenant context middleware for automatic isolation

---

## Architecture

### Architecture Pattern: Clean Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Hrm.Api (Entry Point)                   │
│         Controllers, Program.cs, Configuration               │
└─────────────────────────────────────────────────────────────┘
                              ↑
        ┌─────────────────────┼─────────────────────┐
        ↓                     ↓                     ↓
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│  User Management │ │  Shared Domain   │ │ Shared            │
│  (Domain Layer)  │ │  (Abstractions)  │ │ Infrastructure    │
│                  │ │                  │ │ (Data Access)     │
│ - User           │ │ - IDbContext     │ │ - HrmDbContext    │
│ - Role           │ │ - IRepository    │ │ - UnitOfWork      │
│ - Permission     │ │ - IUnitOfWork    │ │ - Configurations  │
│ - Agent          │ │ - Exceptions     │ │ - Middleware      │
│ - Company        │ │ - DTOs           │ │ - Extensions      │
└──────────────────┘ └──────────────────┘ └──────────────────┘
        ↑                     ↑                     ↑
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              ↓
              ┌──────────────────────────────┐
              │    UserManagement            │
              │    (Application Layer)       │
              │                              │
              │ - Services                   │
              │ - ViewModels/DTOs            │
              │ - Commands (MediatR)         │
              │ - AutoMapper Profiles        │
              └──────────────────────────────┘
```

### Layered Architecture

1. **Hrm.Api (Presentation Layer)**
   - API Controllers
   - Dependency Injection Configuration
   - Middleware Setup
   - Request/Response Handling

2. **UserManagement.Application**
   - Business Logic & Services
   - DTOs & ViewModels
   - MediatR Commands & Queries
   - AutoMapper Profiles

3. **UserManagement.Domain**
   - Entity Definitions
   - Domain Events
   - Value Objects
   - Domain Rules & Validations

4. **Shared.Infrastructure (Data Access)**
   - EF Core DbContext (HrmDbContext)
   - Entity Configurations
   - Repository Pattern
   - Unit of Work Pattern
   - Query Filters (Tenant Isolation)

5. **Shared.Domain**
   - Common Abstractions
   - Base Classes
   - Exception Types
   - DTOs & Response Models

---

## Technology Stack

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | .NET | 10 | Cross-platform runtime |
| **Web API** | ASP.NET Core | 10 | RESTful API framework |
| **ORM** | Entity Framework Core | 10 | Database abstraction |
| **Database** | SQL Server | 2019+ | Relational database |
| **Authentication** | JWT (System.IdentityModel.Tokens.Jwt) | 7+ | Token-based auth |
| **Password Hashing** | PBKDF2 (System.Security.Cryptography) | Built-in | Secure password storage |
| **Auto Mapping** | AutoMapper | 13+ | Object-to-object mapping |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection | Built-in | Service container |
| **Configuration** | Microsoft.Extensions.Configuration | Built-in | App settings management |
| **Logging** | Microsoft.Extensions.Logging | Built-in | Application logging |
| **API Documentation** | Swagger/OpenAPI | Via Swashbuckle | Interactive API docs |
| **Validation** | Data Annotations | Built-in | Model validation |
| **Serialization** | System.Text.Json | Built-in | JSON serialization |

---

## Project Structure

```
D:\ARMAN\SaasProject\
│
├── Src/
│   ├── Product/
│   │   ├── Hrm.Api/                          # Main API Entry Point
│   │   │   ├── Controllers/                  # API Endpoints (Phase 1-5)
│   │   │   ├── Extensions/                   # DI & Configuration Extensions
│   │   │   ├── SeedData/                     # Initial Data Seeding
│   │   │   ├── Program.cs                    # Application Entry Point
│   │   │   ├── appsettings.json              # Configuration
│   │   │   ├── appsettings.Development.json  # Dev Configuration
│   │   │   └── Properties/launchSettings.json# Launch Configuration
│   │   │
│   │   └── School.Api/                       # (Secondary API)
│   │
│   ├── UserManagement/
│   │   ├── UserManagement.Domain/            # Domain Layer
│   │   │   ├── Entities/                     # Domain Models
│   │   │   │   ├── User.cs
│   │   │   │   ├── Role.cs
│   │   │   │   ├── Permission.cs
│   │   │   │   ├── Agent.cs
│   │   │   │   ├── Company.cs
│   │   │   │   ├── Module.cs
│   │   │   │   └── BaseEntities/
│   │   │   │       └── AuditableEntity.cs    # Base with CompanyId
│   │   │   └── Enums/                        # Domain Enumerations
│   │   │
│   │   ├── UserManagement.Application/       # Application Layer
│   │   │   ├── Services/                     # Business Logic Services
│   │   │   │   ├── UserService.cs
│   │   │   │   ├── RoleService.cs
│   │   │   │   ├── AgentService.cs
│   │   │   │   ├── TokenService.cs
│   │   │   │   └── AuthenticationService.cs
│   │   │   ├── DTOs/                         # Request/Response Models
│   │   │   ├── ViewModels/                   # VM for responses
│   │   │   ├── Mappings/                     # AutoMapper Profiles
│   │   │   ├── Commands/                     # MediatR Commands (if used)
│   │   │   └── Queries/                      # MediatR Queries (if used)
│   │   │
│   │   ├── UserManagement.Infrastructure/    # Infrastructure (optional)
│   │   └── UserManagement.Api/               # (Legacy/Secondary API)
│   │
│   └── Shared/
│       ├── Shared.Domain/                    # Shared Abstractions
│       │   ├── Abstractions/
│       │   │   ├── IDbContext.cs
│       │   │   ├── IRepository.cs
│       │   │   ├── IUnitOfWork.cs
│       │   │   ├── ITenantContext.cs
│       │   │   ├── IDbContextTransaction.cs
│       │   │   └── IInitialiser.cs
│       │   ├── BaseEntities/
│       │   ├── DTOs/
│       │   │   └── Response/
│       │   │       └── Response.cs            # Generic Response<T>
│       │   └── Exceptions/
│       │       ├── NotFoundException.cs
│       │       ├── UnauthorizedException.cs
│       │       └── ValidationException.cs
│       │
│       └── Shared.Infrastructure/            # Data Access Layer
│           ├── Data/
│           │   ├── HrmDbContext/
│           │   │   └── HrmDbContext.cs       # Main DbContext
│           │   ├── Configurations/           # EF Core Entity Configurations
│           │   │   ├── User/
│           │   │   ├── Role/
│           │   │   ├── Agent/
│           │   │   ├── Company/
│           │   │   ├── Module/
│           │   │   └── ... other entities
│           │   └── Repositories/
│           │       ├── Repository.cs         # Generic Repository
│           │       └── UnitOfWork.cs         # Unit of Work Pattern
│           ├── Middleware/
│           │   ├── TenantResolutionMiddleware.cs  # Tenant context setup
│           │   └── ExceptionHandlingMiddleware.cs # Global exception handler
│           ├── Persistence/
│           │   └── DbContextInitialiser.cs   # Seeding logic
│           └── Extensions/
│               ├── ServiceCollectionExtensions.cs
│               └── ApplicationBuilderExtensions.cs
│
└── Tests/
    ├── UserManagement.Tests/                 # Unit Tests
    ├── Integration.Tests/                    # Integration Tests
    └── Api.Tests/                            # API Tests (if needed)
```

---

## Prerequisites & Setup

### System Requirements

- **OS**: Windows 10/11, macOS, or Linux
- **.NET**: .NET 10 SDK (https://dotnet.microsoft.com/download)
- **Visual Studio**: VS Community 2026+ OR VS Code
- **SQL Server**: SQL Server 2019+ or SQL Server Express
- **Git**: Git CLI for version control

### Installation Steps

#### 1. Install .NET 10 SDK
```bash
# Verify installation
dotnet --version

# Expected output: 10.0.x
```

#### 2. Install SQL Server
- **Option A**: Download SQL Server Express from Microsoft
- **Option B**: Use Docker: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 mcr.microsoft.com/mssql/server:latest`

#### 3. Clone the Repository
```bash
git clone https://github.com/ArmanOfficial786/CoreSaas.git
cd D:\ARMAN\SaasProject
```

#### 4. Install Dependencies
```bash
# Restore NuGet packages
dotnet restore
```

#### 5. Build the Solution
```bash
# Build all projects
dotnet build

# Expected output: Build succeeded with no errors
```

---

## Database Setup

### Step 1: Connection String Configuration

**File**: `Src/Product/Hrm.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "HrmConnection": "Server=.;Database=HrmDb;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Replace values:
- `Server=.` → Your SQL Server instance (. = local, or use server name)
- `Database=HrmDb` → Your database name
- `Integrated Security=true` → Use Windows authentication (set to false if using SQL auth)
- `User ID=sa; Password=YourPassword` → Add if using SQL authentication

### Step 2: Apply Migrations

```bash
# Navigate to Infrastructure project
cd Src/Shared/Shared.Infrastructure

# Add new migration (if needed)
dotnet ef migrations add InitialCreate --startup-project ../../Product/Hrm.Api

# Apply migration to database
dotnet ef database update --startup-project ../../Product/Hrm.Api

# Expected output: Applying migration '20240101000000_InitialCreate'
```

### Step 3: Database Schema

**Auto-Generated Tables**:
- `dbo.[user]` - User accounts
- `dbo.[role]` - Roles per company
- `dbo.[permission]` - Permissions
- `dbo.[role_module_permissions]` - Role-Permission mapping
- `dbo.[user_role]` - User-Role mapping
- `dbo.[agent]` - Business agents/locations
- `dbo.[company]` - Tenant companies
- `dbo.[module]` - System modules
- `dbo.[module_permission]` - Module-Permission mapping
- `dbo.[user_module_permission]` - User-Permission mapping
- `dbo.[user_status]` - User status tracking
- `dbo.[login_log]` - Login history
- `dbo.[agent_user]` - Agent-User mapping

### Step 4: Seed Initial Data

**Files**:
- `Src/Product/Hrm.Api/SeedData/AuthData/admin.json`
- `Src/Product/Hrm.Api/SeedData/AuthData/tenant.json`
- `Src/Product/Hrm.Api/SeedData/AuthData/permissions.json`

**Seeding Logic** in: `Src/Shared/Shared.Infrastructure/Persistence/DbContextInitialiser.cs`

When you run the API, it automatically:
1. Creates database if not exists
2. Applies all migrations
3. Seeds initial data from JSON files
4. Creates default admin user and company

---

## Running the Project

### Method 1: Visual Studio

1. Open `D:\ARMAN\SaasProject` in Visual Studio
2. Right-click Solution → Restore NuGet Packages
3. Set `Hrm.Api` as Startup Project
4. Press `F5` or click Run
5. Browser opens to `https://localhost:5001/swagger`

### Method 2: Command Line

```bash
# Navigate to API project
cd Src/Product/Hrm.Api

# Run the API
dotnet run

# Output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7000
#       Now listening on: http://localhost:5000
```

### Verify API is Running

```bash
# Test health check
curl -X GET "https://localhost:7000/health" --insecure

# Or visit in browser
# https://localhost:7000/swagger
```

---

## API Implementation Phases

### Phase 1: Authentication & Foundation (Week 1)

**Objectives**:
- JWT token generation and validation
- User authentication (login/logout)
- Tenant context middleware
- Permission-based authorization

**Endpoints to Create**:
```
POST   /api/auth/login              - User login
POST   /api/auth/logout             - User logout
GET    /api/auth/profile            - Get current user
POST   /api/auth/refresh            - Refresh token
POST   /api/auth/validate           - Validate token
```

**Key Files**:
- Create: `Src/UserManagement/UserManagement.Application/Services/TokenService.cs`
- Create: `Src/UserManagement/UserManagement.Application/Services/AuthenticationService.cs`
- Create: `Src/Product/Hrm.Api/Controllers/AuthController.cs`
- Update: `Src/Product/Hrm.Api/Program.cs` (JWT configuration)

---

### Phase 2: Master Data APIs (Week 2)

**Objectives**:
- Company management
- Agent/Location management
- Module and Module-Permission setup

**Endpoints to Create**:
```
GET    /api/company/{id}            - Get company
POST   /api/company                 - Create company
PUT    /api/company/{id}            - Update company

GET    /api/agent                   - List agents
GET    /api/agent/{id}              - Get agent
POST   /api/agent                   - Create agent
PUT    /api/agent/{id}              - Update agent
DELETE /api/agent/{id}              - Delete agent

GET    /api/module                  - List modules
GET    /api/module/{id}             - Get module
```

**Key Files**:
- Create: `Src/UserManagement/UserManagement.Application/Services/CompanyService.cs`
- Create: `Src/UserManagement/UserManagement.Application/Services/AgentService.cs`
- Create: `Src/Product/Hrm.Api/Controllers/CompanyController.cs`
- Create: `Src/Product/Hrm.Api/Controllers/AgentController.cs`

---

### Phase 3: User Management (Week 3)

**Objectives**:
- User CRUD operations
- User role assignment
- User activation/deactivation
- Login attempt tracking

**Endpoints to Create**:
```
POST   /api/user/login              - User login (uses auth service)
GET    /api/user/profile            - Get current user
POST   /api/user                    - Create user
GET    /api/user                    - List users
GET    /api/user/{id}               - Get user
PUT    /api/user/{id}               - Update user
DELETE /api/user/{id}               - Delete user

POST   /api/user/{id}/roles         - Assign role
GET    /api/user/{id}/roles         - Get user roles
DELETE /api/user/{id}/roles/{roleId}- Remove role
```

**Key Files**:
- Create: `Src/UserManagement/UserManagement.Application/Services/UserService.cs`
- Create: `Src/Product/Hrm.Api/Controllers/UserController.cs`
- Create DTOs: `Src/UserManagement/UserManagement.Application/DTOs/UserDtos.cs`

---

### Phase 4: Role & Permission Management (Week 4)

**Objectives**:
- Role CRUD operations
- Permission management
- Role-Permission mapping
- Dynamic permission checking

**Endpoints to Create**:
```
GET    /api/role                    - List roles
POST   /api/role                    - Create role
GET    /api/role/{id}               - Get role
PUT    /api/role/{id}               - Update role
DELETE /api/role/{id}               - Delete role

POST   /api/role/{id}/permissions   - Assign permission
GET    /api/role/{id}/permissions   - Get role permissions
DELETE /api/role/{id}/permissions/{permId} - Remove permission

GET    /api/permission              - List permissions
POST   /api/permission              - Create permission
```

**Key Files**:
- Create: `Src/UserManagement/UserManagement.Application/Services/RoleService.cs`
- Create: `Src/UserManagement/UserManagement.Application/Services/PermissionService.cs`
- Create: `Src/Product/Hrm.Api/Controllers/RoleController.cs`
- Create: `Src/Product/Hrm.Api/Controllers/PermissionController.cs`

---

### Phase 5: Advanced Features (Week 5)

**Objectives**:
- User permissions verification
- Agent-User assignment
- Login logging & audit trail
- Error handling & validation
- API documentation

**Endpoints to Create**:
```
GET    /api/user/permissions        - Get user permissions
POST   /api/user/permissions/check  - Check specific permission

POST   /api/agent/{id}/users        - Assign user to agent
GET    /api/agent/{id}/users        - Get agent users
DELETE /api/agent/{id}/users/{userId} - Remove user

GET    /api/audit/login-logs        - View login history
GET    /api/audit/activities        - View activity logs
```

**Key Features**:
- Global exception handling middleware
- Request validation middleware
- Request/response logging
- API rate limiting (optional)
- CORS configuration
- Health check endpoint

---

## Next Steps

### Immediate Actions (Now)

1. **✅ Verify Setup**
   ```bash
   dotnet build
   dotnet run --project Src/Product/Hrm.Api
   ```

2. **✅ Check Database Connection**
   - Ensure SQL Server is running
   - Verify connection string in `appsettings.json`
   - Run migrations

3. **✅ Test Swagger UI**
   - Navigate to `https://localhost:7000/swagger`
   - Should see empty endpoints (Phase 1 not yet implemented)

### Week 1 Tasks (Phase 1: Authentication)

**Day 1-2: Setup**
- [ ] Create `AuthController.cs` with login endpoint
- [ ] Create `TokenService.cs` for JWT generation
- [ ] Create `AuthenticationService.cs` with password hashing
- [ ] Create DTOs: `LoginRequest`, `AuthResponse`, `TokenClaims`

**Day 3-4: Implementation**
- [ ] Implement JWT configuration in `Program.cs`
- [ ] Add authentication middleware
- [ ] Test login endpoint with Postman/Insomnia
- [ ] Implement token refresh logic

**Day 5: Testing & Documentation**
- [ ] Test with multiple users
- [ ] Add Swagger documentation
- [ ] Test token expiration
- [ ] Create Postman collection

### Week 2 Tasks (Phase 2: Master Data)

- [ ] Create `CompanyController` & `CompanyService`
- [ ] Create `AgentController` & `AgentService`
- [ ] Add pagination to list endpoints
- [ ] Implement soft delete on agents
- [ ] Test CompanyId filtering

### Week 3 Tasks (Phase 3: User Management)

- [ ] Create `UserService` with CRUD operations
- [ ] Create `UserController` with all user endpoints
- [ ] Implement user role assignment
- [ ] Create `UserViewModel` for responses
- [ ] Setup AutoMapper profiles

### Week 4 Tasks (Phase 4: Permissions)

- [ ] Create `RoleService` & `RoleController`
- [ ] Create `PermissionService` & `PermissionController`
- [ ] Implement role-permission assignment
- [ ] Setup permission-based authorization
- [ ] Create custom authorization policies

### Week 5 Tasks (Phase 5: Advanced)

- [ ] Create exception handling middleware
- [ ] Add validation middleware
- [ ] Implement audit logging
- [ ] Add health check endpoint
- [ ] Setup CORS properly
- [ ] Write unit tests
- [ ] Write integration tests

---

## Configuration Guide

### JWT Configuration

**appsettings.json**:
```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-minimum-32-characters-long-here",
    "Issuer": "HrmApi",
    "Audience": "HrmUsers",
    "ExpirationMinutes": 60
  }
}
```

**Program.cs**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"]);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });
```

### Tenant Resolution

**Automatic Tenant Detection**:
The `TenantResolutionMiddleware` automatically:
1. Reads `CompanyId` from JWT claims
2. Sets `ITenantContext` for the request
3. All queries automatically filtered by `CompanyId`
4. All new entities automatically stamped with `CompanyId`

### Multi-Tenant Data Isolation

**Every read query is filtered**:
```csharp
// Automatic: WHERE CompanyId = currentTenantId
var users = await repository.GetAllAsync(); // Only current tenant's users
```

**Every new entity is tagged**:
```csharp
var user = new User(...);
// Automatically: user.CompanyId = currentTenantContext.CompanyId
await unitOfWork.CommitAsync();
```

---

## Testing the API

### Manual Testing with cURL

```bash
# 1. Login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password123"}' \
  -k

# Response:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "expiresAt": "2024-12-31T10:00:00Z",
#   "user": { ... }
# }

# 2. Use token to access protected endpoint
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET "https://localhost:7000/api/user/profile" \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

### Automated Testing with Postman

1. Import provided Postman collections
2. Set up environment variables:
   - `base_url`: `https://localhost:7000`
   - `token`: (auto-populated after login)
3. Run requests in order
4. Verify responses match expected schemas

### Unit Testing

Create tests for each service:
```csharp
public class UserServiceTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var service = new AuthenticationService(mockUnitOfWork.Object, /* ... */);

        // Act
        var result = await service.LoginAsync(new LoginRequest 
        { 
            Email = "user@example.com", 
            Password = "password123" 
        });

        // Assert
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
    }
}
```

---

## Troubleshooting

### Issue 1: Database Connection Failed

**Error**: `A network-related or instance-specific error...`

**Solution**:
1. Verify SQL Server is running: `Services.msc` (Windows) or `sudo systemctl status mssql-server` (Linux)
2. Check connection string in `appsettings.json`
3. Test connection: `sqlcmd -S . -U sa -P YourPassword`

### Issue 2: Migrations Not Found

**Error**: `The migrations assembly in the 'Shared.Infrastructure' project could not be loaded`

**Solution**:
```bash
# Ensure you're in the correct directory
cd Src/Shared/Shared.Infrastructure

# Clean and rebuild
dotnet clean
dotnet build

# Try migration again
dotnet ef database update --startup-project ../../Product/Hrm.Api
```

### Issue 3: JWT Token Invalid

**Error**: `Authorization header with scheme 'Bearer' was not found`

**Solution**:
1. Ensure you're including `Authorization: Bearer {token}` header
2. Verify token hasn't expired
3. Check JWT secret key matches in configuration
4. Verify token was generated with correct claims

### Issue 4: CompanyId Not Being Set

**Error**: `The instance of entity type 'User' cannot be tracked because another instance with the same key is already being tracked`

**Solution**:
1. Verify `TenantResolutionMiddleware` is registered
2. Check middleware order: Auth → Authorization → Tenant Resolution
3. Ensure `ITenantContext` is properly injected
4. Call `StampTenant()` in `SaveChangesAsync()`

### Issue 5: Swagger UI Not Loading

**Error**: `localhost refused to connect` at `/swagger`

**Solution**:
1. Verify API is running
2. Check port number in `Properties/launchSettings.json`
3. Ensure `UseSwagger()` and `UseSwaggerUI()` are called in Program.cs
4. Clear browser cache

### Issue 6: CORS Errors

**Error**: `Access to XMLHttpRequest... has been blocked by CORS policy`

**Solution**:
Add CORS configuration in Program.cs:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

---

## Performance Optimization

### Database Performance

1. **Add Indexes**:
   ```csharp
   modelBuilder.Entity<User>()
       .HasIndex(u => u.Email)
       .IsUnique()
       .HasFilter("[CompanyId] IS NOT NULL");

   modelBuilder.Entity<User>()
       .HasIndex(u => new { u.CompanyId, u.Email })
       .IsUnique();
   ```

2. **Query Optimization**:
   ```csharp
   // Bad: N+1 query problem
   var users = await repository.GetAllAsync();
   foreach (var user in users)
   {
       var roles = user.UserRoles; // Additional query per user
   }

   // Good: Use Include for eager loading
   var users = await repository.GetAllAsync(
       includeProperties: new[] { nameof(User.UserRoles) }
   );
   ```

3. **Pagination**:
   ```csharp
   var pagedUsers = await userService.GetUsersAsync(
       pageNumber: 1,
       pageSize: 50
   );
   ```

### API Performance

1. **Response Compression**:
   ```csharp
   builder.Services.AddResponseCompression();
   app.UseResponseCompression();
   ```

2. **Caching**:
   ```csharp
   builder.Services.AddMemoryCache();

   // In service
   public async Task<UserDto> GetUserAsync(Guid id)
   {
       var cacheKey = $"user_{id}";
       if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
           return cachedUser;

       var user = await repository.GetByIdAsync(id);
       _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
       return user;
   }
   ```

3. **Async/Await**:
   - Use async operations throughout
   - Avoid blocking calls (`Result`, `Wait()`)
   - Use `async Task` for void-returning methods

---

## Security Considerations

### Authentication Security

- ✅ Use HTTPS in production
- ✅ Hash passwords with PBKDF2 (minimum 10,000 iterations)
- ✅ Validate JWT tokens on every request
- ✅ Set appropriate token expiration (60 minutes)
- ✅ Implement refresh token mechanism
- ✅ Log failed login attempts

### Authorization Security

- ✅ Always check `CompanyId` on multi-tenant operations
- ✅ Verify user has required role/permission
- ✅ Use `[Authorize]` attributes on all endpoints
- ✅ Implement permission-based authorization
- ✅ Never trust client-provided CompanyId

### Data Security

- ✅ Use parameterized queries (EF Core does this)
- ✅ Validate all input data
- ✅ Implement soft delete (ToDate field)
- ✅ Audit all sensitive operations
- ✅ Encrypt sensitive data at rest (optional)
- ✅ Use HTTPS for transport security

### API Security

- ✅ Implement rate limiting
- ✅ Set proper CORS policies
- ✅ Add request size limits
- ✅ Validate Content-Type headers
- ✅ Implement CSRF protection
- ✅ Add security headers (X-Frame-Options, etc.)

---

## Deployment

### Pre-Deployment Checklist

- [ ] Database migrations tested on production schema
- [ ] All secrets in environment variables (not in code)
- [ ] JWT secret key changed from default
- [ ] HTTPS certificate installed
- [ ] CORS configured for production domain
- [ ] Logging configured appropriately
- [ ] Health check endpoint working
- [ ] Database backups automated
- [ ] Monitoring/alerting configured
- [ ] Load testing completed

### Deployment Steps

```bash
# 1. Build release version
dotnet publish -c Release -o ./release

# 2. Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__HrmConnection="Server=prod-server;Database=HrmDb;..."
export Jwt__SecretKey="production-secret-key-here"

# 3. Run API
cd release
dotnet Hrm.Api.dll

# 4. Verify deployment
curl https://prod.example.com/api/auth/login
```

---

## Support & Resources

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [JWT Documentation](https://tools.ietf.org/html/rfc7519)

### Tools
- [Postman](https://www.postman.com/) - API testing
- [Insomnia](https://insomnia.rest/) - API testing alternative
- [SQL Server Management Studio](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) - Database management
- [Git](https://git-scm.com/) - Version control

### Community
- [Stack Overflow](https://stackoverflow.com/questions/tagged/asp.net-core)
- [Microsoft Q&A](https://docs.microsoft.com/answers/topics/dotnet.html)
- [GitHub Discussions](https://github.com/ArmanOfficial786/CoreSaas/discussions)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 2024 | Initial setup documentation |
| 1.1 | - | To be updated as project evolves |

---

## Contact & Contributions

**Project**: CoreSaas HRM System  
**Repository**: https://github.com/ArmanOfficial786/CoreSaas  
**Issues**: https://github.com/ArmanOfficial786/CoreSaas/issues  

For questions or contributions, please open an issue or pull request on GitHub.

---

**Last Updated**: December 2024  
**Status**: Complete  
**Next Review**: After Phase 1 Implementation
