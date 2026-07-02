# HRM SaaS - Next Steps & Action Plan

## Document Purpose

This document provides clear, actionable next steps for the HRM SaaS project starting from the current state. It includes:
- Immediate actions (today)
- Week 1-5 implementation roadmap
- Detailed task breakdown
- Success criteria for each phase
- Risk mitigation strategies

---

## Current Project State

### ✅ Completed
- Project structure setup
- Multi-tenant architecture (CompanyId-based isolation)
- Domain entities created (User, Role, Permission, Agent, Company, Module)
- Shared infrastructure layer configured
- EF Core DbContext with tenant filtering
- Database migrations framework ready
- Swagger/OpenAPI configuration ready
- Seed data structure in place

### 🔄 In Progress
- API implementation (controllers, services)
- Authentication & authorization
- Business logic services

### ⏳ Pending
- Phase 1: Complete Authentication APIs
- Phase 2: Master data APIs
- Phase 3: User management APIs
- Phase 4: Role & permission APIs
- Phase 5: Advanced features & testing

---

## Immediate Actions (Today/This Week)

### Step 1: Verify Environment & Database

**Time**: 30 minutes

```bash
# 1.1 Check .NET version
dotnet --version
# Expected: 10.0.x

# 1.2 Verify SQL Server is running
# On Windows: Check Services.msc for SQL Server
# On Linux: sudo systemctl status mssql-server
# On Docker: docker ps (look for mssql container)

# 1.3 Test database connection
# Open SQL Server Management Studio or
# Use: sqlcmd -S . -U sa -P YourPassword

# 1.4 Build project
cd D:\ARMAN\SaasProject
dotnet clean
dotnet build

# Expected output: Build succeeded
```

**Troubleshooting**:
- If SQL Server not found: Download from https://www.microsoft.com/sql-server/sql-server-downloads
- If build fails: Run `dotnet restore` first

---

### Step 2: Setup Database

**Time**: 20 minutes

```bash
# 2.1 Navigate to Infrastructure project
cd Src/Shared/Shared.Infrastructure

# 2.2 Create initial migration
dotnet ef migrations add InitialCreate \
  --startup-project ../../Product/Hrm.Api \
  --context HrmDbContext

# Expected output: Migration 'InitialCreate' created

# 2.3 Apply migration to database
dotnet ef database update \
  --startup-project ../../Product/Hrm.Api

# Expected output: 
# info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
#       Entity Framework Core initialized 'HrmDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: None
# info: Microsoft.EntityFrameworkCore.Database.Command[20101]
#       Executed DbCommand (250ms) [Parameters=[], CommandType='Text']
#       CREATE DATABASE [HrmDb]
# ... (more migrations applied)
```

**Verify**:
- Open SQL Server Management Studio
- Connect to your SQL Server
- Expand Databases → HrmDb
- Should see tables: user, role, permission, agent, company, etc.

---

### Step 3: Run Application & Test Seeding

**Time**: 15 minutes

```bash
# 3.1 Navigate to API project
cd Src/Product/Hrm.Api

# 3.2 Run application
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7000
#       Now listening on: http://localhost:5000
```

**Verify**:
- Open browser to `https://localhost:7000/swagger`
- Should see Swagger UI with empty endpoint list (expected)
- Should see seed data applied in database
  - Admin user created
  - Default company created
  - Default roles created

---

### Step 4: Review Architecture & Code

**Time**: 45 minutes

Read these files in order:

1. **Src/Shared/Shared.Domain/Abstractions/ITenantContext.cs**
   - Understand tenant context mechanism

2. **Src/UserManagement/UserManagement.Domain/Entities/User.cs**
   - Review User entity structure

3. **Src/Shared/Shared.Infrastructure/Data/HrmDbContext/HrmDbContext.cs**
   - Study how tenant filtering works
   - Look at `ApplyTenantQueryFiltersByConvention()` method
   - Look at `StampTenant()` method

4. **Src/Shared/Shared.Infrastructure/TenantResolutionMiddleware.cs**
   - Understand how CompanyId is extracted from JWT

5. **Src/Product/Hrm.Api/Program.cs**
   - Review service registration

---

## Phase 1: Authentication & Foundation (Week 1)

### Objective
Implement JWT-based authentication with login, token refresh, and user profile endpoints.

### Endpoints to Build

| Method | Endpoint | Purpose | Auth Required |
|--------|----------|---------|---------------|
| POST | `/api/auth/login` | User login | No |
| POST | `/api/auth/logout` | User logout | Yes |
| POST | `/api/auth/refresh` | Refresh token | Yes |
| GET | `/api/auth/profile` | Get current user | Yes |
| GET | `/api/auth/validate` | Validate token | Yes |

### Day-by-Day Breakdown

#### Day 1: Setup Services & DTOs

**Create Files**:
1. `Src/UserManagement/UserManagement.Application/DTOs/AuthDtos.cs`
```csharp
// LoginRequest, AuthResponse, TokenClaims, RefreshTokenRequest
```

2. `Src/UserManagement/UserManagement.Application/Services/TokenService.cs`
```csharp
// GenerateToken(), ValidateToken(), RefreshToken()
```

3. `Src/UserManagement/UserManagement.Application/Services/AuthenticationService.cs`
```csharp
// LoginAsync(), LogoutAsync(), ValidateCredentialsAsync(), GetUserPermissionsAsync()
```

**Tasks**:
- [ ] Create DTOs for auth requests/responses
- [ ] Implement TokenService with JWT generation
- [ ] Implement AuthenticationService with password hashing
- [ ] Add services to dependency injection in Program.cs

**Success Criteria**:
- Code compiles without errors
- TokenService generates valid JWT tokens
- AuthenticationService validates credentials correctly

---

#### Day 2: Create AuthController

**Create Files**:
1. `Src/Product/Hrm.Api/Controllers/AuthController.cs`

**Controller Methods**:
```csharp
[AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    // Validate model
    // Call authentication service
    // Return token and user info
}

[Authorize]
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    // Log logout event
    // Optional: Blacklist token (advanced)
}

[Authorize]
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
{
    // Validate refresh token
    // Generate new access token
}

[Authorize]
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    // Get current user from claims
    // Return user profile with roles
}

[AllowAnonymous]
[HttpGet("validate")]
public async Task<IActionResult> ValidateToken([FromQuery] string token)
{
    // Validate token format and signature
    // Return validation result
}
```

**Tasks**:
- [ ] Create AuthController class
- [ ] Implement login endpoint
- [ ] Implement logout endpoint
- [ ] Implement refresh endpoint
- [ ] Implement profile endpoint
- [ ] Implement validate endpoint
- [ ] Add error handling

**Success Criteria**:
- Swagger shows all 5 endpoints
- Login works with correct credentials
- Login fails with incorrect credentials
- Token can be used to access protected endpoints
- Refresh token generates new token

---

#### Day 3: JWT Configuration & Middleware

**Update Files**:
1. `Src/Product/Hrm.Api/appsettings.json`
```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-minimum-32-characters-long",
    "Issuer": "HrmApi",
    "Audience": "HrmUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

2. `Src/Product/Hrm.Api/Program.cs`
- Add JWT authentication
- Add authorization
- Configure middleware order

**Tasks**:
- [ ] Configure JWT settings in appsettings.json
- [ ] Add authentication to Program.cs
- [ ] Add authorization to Program.cs
- [ ] Add token validation middleware
- [ ] Ensure middleware order is correct

**Middleware Order** (important!):
```csharp
app.UseHttpsRedirection();        // 1. HTTPS
app.UseAuthentication();           // 2. Authentication
app.UseAuthorization();            // 3. Authorization
app.UseMiddleware<TenantResolutionMiddleware>(); // 4. Tenant
app.MapControllers();              // 5. Routes
```

**Success Criteria**:
- JWT configuration loaded correctly
- Middleware executes in correct order
- Unauthorized requests get 401 response
- Authorized requests succeed

---

#### Day 4: Testing & Debugging

**Test Cases**:

1. **Test Login Success**:
   ```bash
   curl -X POST https://localhost:7000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@example.com","password":"admin"}' \
     -k

   # Expect 200 with token
   ```

2. **Test Login Failure - Wrong Password**:
   ```bash
   curl -X POST https://localhost:7000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@example.com","password":"wrong"}' \
     -k

   # Expect 401 Unauthorized
   ```

3. **Test Protected Endpoint Without Token**:
   ```bash
   curl -X GET https://localhost:7000/api/auth/profile -k

   # Expect 401 Unauthorized
   ```

4. **Test Protected Endpoint With Token**:
   ```bash
   TOKEN="<token from login response>"
   curl -X GET https://localhost:7000/api/auth/profile \
     -H "Authorization: Bearer $TOKEN" \
     -k

   # Expect 200 with user profile
   ```

5. **Test Token Refresh**:
   ```bash
   curl -X POST https://localhost:7000/api/auth/refresh \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{"refreshToken":"<refresh_token>"}' \
     -k

   # Expect 200 with new token
   ```

**Tasks**:
- [ ] Test all endpoints with cURL
- [ ] Test with Postman/Insomnia
- [ ] Test error scenarios
- [ ] Verify error messages are helpful

**Success Criteria**:
- All test cases pass
- Error responses are consistent
- Token contains correct claims
- Middleware correctly extracts CompanyId from token

---

#### Day 5: Documentation & Polish

**Tasks**:
- [ ] Add XML documentation to all public methods
- [ ] Update Swagger descriptions
- [ ] Create Postman collection for auth endpoints
- [ ] Write README for Phase 1
- [ ] Test with real frontend scenarios

**Deliverables**:
1. `Postman/Auth_Phase1.postman_collection.json`
   - Import login request
   - Import profile request
   - Import refresh request
   - Setup environment variables

2. XML Documentation in code:
```csharp
/// <summary>
/// Authenticates user with email and password
/// </summary>
/// <param name="request">Login credentials</param>
/// <returns>JWT token and user profile</returns>
/// <response code="200">Successful login</response>
/// <response code="401">Invalid credentials</response>
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
```

**Success Criteria**:
- Swagger shows descriptions for all endpoints
- Code is well-documented
- Postman collection provided
- Phase 1 summary document created

---

### Phase 1 Success Criteria

- ✅ Login endpoint returns JWT token
- ✅ Token contains user info and CompanyId
- ✅ Protected endpoints require valid token
- ✅ Invalid credentials return 401
- ✅ Token can be refreshed
- ✅ Profile endpoint returns current user
- ✅ All endpoints documented in Swagger
- ✅ Error handling is consistent
- ✅ Middleware order is correct

---

## Phase 2: Master Data APIs (Week 2)

### Objective
Implement Company, Agent, and Module management endpoints with pagination and filtering.

### Endpoints Overview

**Company**:
- GET `/api/company/{id}` - Get company
- POST `/api/company` - Create company
- PUT `/api/company/{id}` - Update company

**Agent**:
- GET `/api/agent` - List agents (paginated)
- GET `/api/agent/{id}` - Get agent
- POST `/api/agent` - Create agent
- PUT `/api/agent/{id}` - Update agent
- DELETE `/api/agent/{id}` - Delete agent

**Module**:
- GET `/api/module` - List modules

### Implementation Order

1. **Company Service & Controller** (Day 1)
2. **Agent Service & Controller** (Day 2-3)
3. **Module Service & Controller** (Day 4)
4. **Testing & Documentation** (Day 5)

### Key Considerations

- ✅ All operations filtered by CompanyId automatically
- ✅ Implement pagination for list endpoints
- ✅ Use soft delete (ToDate field) for agent deletion
- ✅ Validate CompanyId matches tenant context
- ✅ Add authorization checks (Admin/Manager roles)

---

## Phase 3: User Management (Week 3)

### Objective
Implement user CRUD operations, role assignment, and user activation.

### Endpoints Overview

**User Management**:
- POST `/api/user/login` - Login (uses auth service)
- GET `/api/user/profile` - Get current user
- POST `/api/user` - Create user
- GET `/api/user` - List users (paginated)
- GET `/api/user/{id}` - Get user
- PUT `/api/user/{id}` - Update user
- DELETE `/api/user/{id}` - Delete user (soft)

**User Roles**:
- POST `/api/user/{id}/roles` - Assign role
- GET `/api/user/{id}/roles` - Get user roles
- DELETE `/api/user/{id}/roles/{roleId}` - Remove role

### Implementation Order

1. **User Service & Controller** (Day 1-2)
2. **User Role Assignment** (Day 3)
3. **User Status Management** (Day 4)
4. **Testing & Documentation** (Day 5)

### Key Considerations

- ✅ Email uniqueness per company
- ✅ Password hashing with PBKDF2
- ✅ Failed login attempt tracking
- ✅ Account lockout after failed attempts
- ✅ User status history (UserStatus entity)

---

## Phase 4: Role & Permission Management (Week 4)

### Objective
Implement role and permission management with role-permission mapping.

### Endpoints Overview

**Roles**:
- GET `/api/role` - List roles
- POST `/api/role` - Create role
- GET `/api/role/{id}` - Get role
- PUT `/api/role/{id}` - Update role
- DELETE `/api/role/{id}` - Delete role

**Permissions**:
- GET `/api/permission` - List permissions
- POST `/api/permission` - Create permission

**Role-Permission**:
- POST `/api/role/{id}/permissions` - Assign permission
- GET `/api/role/{id}/permissions` - Get permissions
- DELETE `/api/role/{id}/permissions/{permId}` - Remove permission

### Implementation Order

1. **Role Service & Controller** (Day 1)
2. **Permission Service & Controller** (Day 2)
3. **Role-Permission Assignment** (Day 3)
4. **Permission-based Authorization** (Day 4)
5. **Testing & Documentation** (Day 5)

### Key Considerations

- ✅ Roles are scoped per company
- ✅ Permissions are system-wide but role assignments are per company
- ✅ Check user has Admin role before modifying roles/permissions
- ✅ Implement permission-based authorization policies

---

## Phase 5: Advanced Features & Polish (Week 5)

### Objective
Complete missing features, add validation, logging, and comprehensive testing.

### Features to Implement

1. **User Permissions API**
   - GET `/api/user/permissions` - Get all user permissions
   - POST `/api/user/permissions/check` - Check specific permission

2. **Agent-User Assignment**
   - POST `/api/agent/{id}/users` - Assign user to agent
   - GET `/api/agent/{id}/users` - Get agent users
   - DELETE `/api/agent/{id}/users/{userId}` - Remove user

3. **Audit & Logging**
   - GET `/api/audit/login-logs` - View login history
   - GET `/api/audit/activities` - View activity logs

4. **Error Handling**
   - Global exception handling middleware
   - Custom exception types
   - Consistent error response format

5. **Validation**
   - FluentValidation for all DTOs
   - Model state validation
   - Business logic validation

6. **Testing**
   - Unit tests for services
   - Integration tests for APIs
   - End-to-end tests

### Implementation Order

1. **Exception Handling & Validation** (Day 1)
2. **User Permissions & Audit** (Day 2-3)
3. **Unit Tests** (Day 4)
4. **Integration Tests & Polish** (Day 5)

---

## Risk Mitigation

### Risk 1: Database Migration Issues

**Risk**: Migrations fail or corrupt database

**Mitigation**:
- [ ] Always backup database before applying migrations
- [ ] Test migrations on development database first
- [ ] Use migration script for rollback capability
- [ ] Version control migrations in Git

**Rollback Procedure**:
```bash
# Remove latest migration
dotnet ef migrations remove --startup-project ../../Product/Hrm.Api

# Or revert to specific migration
dotnet ef database update 20240101000000_PreviousMigration \
  --startup-project ../../Product/Hrm.Api
```

---

### Risk 2: Multi-Tenant Data Isolation Breach

**Risk**: User can see data from other companies

**Mitigation**:
- [ ] Always filter by CompanyId in queries
- [ ] Never skip tenant filtering
- [ ] Use `TenantContext` automatically
- [ ] Test cross-tenant access scenarios
- [ ] Code review all queries

**Testing**:
```csharp
// Create users in Company A and Company B
// Login as Company A user
// Verify cannot access Company B data
var usersA = await userService.GetUsersAsync();
Assert.All(usersA, u => u.CompanyId == tenantA.Id);
```

---

### Risk 3: JWT Token Expiration Issues

**Risk**: Tokens expire too quickly or don't expire

**Mitigation**:
- [ ] Set appropriate expiration time (60 minutes)
- [ ] Implement refresh token mechanism
- [ ] Test token expiration scenarios
- [ ] Handle expired token errors gracefully

**Testing**:
```csharp
// Generate token with 1 second expiration
var token = tokenService.GenerateToken(claims, TimeSpan.FromSeconds(1));

// Wait 2 seconds
await Task.Delay(2000);

// Verify token is invalid
var result = tokenService.ValidateToken(token);
Assert.Null(result);
```

---

### Risk 4: Performance Degradation

**Risk**: Queries become slow with large datasets

**Mitigation**:
- [ ] Implement pagination (50 items per page)
- [ ] Add database indexes on common columns
- [ ] Use eager loading (Include) for related data
- [ ] Implement caching for frequently accessed data
- [ ] Monitor query performance

**Database Indexes**:
```csharp
// In entity configuration
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();

modelBuilder.Entity<User>()
    .HasIndex(u => new { u.CompanyId, u.Email })
    .IsUnique();

modelBuilder.Entity<Agent>()
    .HasIndex(a => a.CompanyId);
```

---

### Risk 5: Authorization Bypass

**Risk**: User accesses unauthorized endpoints

**Mitigation**:
- [ ] Use `[Authorize]` on all protected endpoints
- [ ] Check roles/permissions in service layer
- [ ] Validate CompanyId on all operations
- [ ] Never trust client-provided CompanyId
- [ ] Test with different user roles

**Code Example**:
```csharp
[Authorize(Roles = "Admin")]
[HttpPost("user")]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // Service also checks CompanyId
    var user = await _userService.CreateUserAsync(request);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
```

---

## Success Metrics

### By End of Week 1 (Phase 1)
- ✅ 5 authentication endpoints working
- ✅ JWT tokens generated and validated
- ✅ Login/logout functionality tested
- ✅ 90%+ code coverage for auth services
- ✅ No security vulnerabilities identified

### By End of Week 2 (Phase 2)
- ✅ 8+ master data endpoints working
- ✅ Pagination implemented
- ✅ Soft delete functional
- ✅ Multi-tenant isolation verified
- ✅ All endpoints documented

### By End of Week 3 (Phase 3)
- ✅ 10+ user management endpoints
- ✅ User creation with hashed passwords
- ✅ Role assignment functionality
- ✅ Failed login attempt tracking
- ✅ Integration tests passing

### By End of Week 4 (Phase 4)
- ✅ Role and permission management complete
- ✅ Permission-based authorization working
- ✅ All RBAC tests passing
- ✅ API documentation complete
- ✅ Postman collections provided

### By End of Week 5 (Phase 5)
- ✅ Exception handling middleware working
- ✅ Validation on all endpoints
- ✅ Audit logging functional
- ✅ 80%+ unit test coverage
- ✅ Project ready for QA testing

---

## Deliverables Checklist

### Code Deliverables
- [ ] AuthController & services
- [ ] CompanyController & services
- [ ] AgentController & services
- [ ] UserController & services
- [ ] RoleController & services
- [ ] PermissionController & services
- [ ] Exception handling middleware
- [ ] Validation middleware

### Documentation Deliverables
- [ ] API_SETUP_GUIDE.md (already created)
- [ ] API_IMPLEMENTATION_CHECKLIST.md (already created)
- [ ] API_CODE_SNIPPETS.md (already created)
- [ ] PROJECT_SETUP_FROM_BEGINNING.md (already created)
- [ ] Phase-specific README files
- [ ] Swagger documentation

### Testing Deliverables
- [ ] Postman collections (Auth, Company, Agent, User, Role, Permission)
- [ ] Unit test suite
- [ ] Integration test suite
- [ ] End-to-end test scenarios

### Deployment Deliverables
- [ ] Database migration scripts
- [ ] Environment configuration templates
- [ ] Deployment guide
- [ ] CI/CD pipeline (optional)

---

## Getting Help

### When Stuck

1. **Check error message carefully** - Most errors have clear solutions
2. **Look at similar implementations** - Reference other controllers/services
3. **Check API_CODE_SNIPPETS.md** - Has complete code examples
4. **Search GitHub issues** - Solution might already exist
5. **Review entity relationships** - Many issues from wrong navigation properties

### Common Issues Solutions

1. **"DbSet not found" error**
   - Add DbSet property to HrmDbContext
   - Remember: `public DbSet<Entity> Entities => Set<Entity>();`

2. **"CompanyId filtering not working"**
   - Verify middleware order in Program.cs
   - Check TenantContext is properly injected
   - Verify entity has CompanyId property

3. **"Token always returns 401"**
   - Check JWT secret key matches
   - Verify token contains CompanyId claim
   - Check Authorization header format: "Bearer {token}"

4. **"Database connection failed"**
   - SQL Server running? Check services
   - Connection string correct? Check appsettings.json
   - Database created? Run migrations

---

## Next Steps - Right Now!

1. ✅ **Verify Setup** (30 min)
   - [ ] Run `dotnet build`
   - [ ] Run `dotnet run`
   - [ ] Test Swagger at https://localhost:7000/swagger

2. ✅ **Setup Database** (20 min)
   - [ ] Run migrations
   - [ ] Verify tables created in SQL Server

3. ✅ **Read Code** (45 min)
   - [ ] Review entity definitions
   - [ ] Understand HrmDbContext
   - [ ] Study middleware

4. 🚀 **Start Phase 1** (Tomorrow)
   - [ ] Create Auth DTOs
   - [ ] Create TokenService
   - [ ] Create AuthenticationService
   - [ ] Create AuthController

---

## Questions?

Refer to:
- PROJECT_SETUP_FROM_BEGINNING.md - Full setup guide
- API_SETUP_GUIDE.md - Architecture and design
- API_CODE_SNIPPETS.md - Code examples
- API_IMPLEMENTATION_CHECKLIST.md - Task checklist

---

**Created**: December 2024  
**Last Updated**: December 2024  
**Status**: Ready for Implementation  
**Estimated Timeline**: 5 Weeks  
**Team Size**: 1-2 developers
