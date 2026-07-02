# API Implementation Checklist

## Phase 1: Authentication & Foundation ✓

### JWT Setup
- [ ] Configure JWT settings in `appsettings.json`
  - SecretKey (min 32 characters)
  - Issuer
  - Audience
  - ExpirationMinutes (default 60)

- [ ] Create `ITokenService` interface
  - Method: `GenerateToken(TokenClaims)`
  - Method: `ValidateToken(string token)`
  - Method: `RefreshToken(string token)`

- [ ] Create `TokenService` implementation
  - Generate JWT with CompanyId claim
  - Include UserId, Email, Roles, Permissions
  - Handle token expiration

- [ ] Create `IAuthenticationService` interface
  - Method: `LoginAsync(LoginRequest)`
  - Method: `RefreshTokenAsync(string refreshToken)`
  - Method: `ValidateTokenAsync(string token)`

- [ ] Create `AuthenticationService` implementation
  - User lookup by email
  - Password verification (bcrypt)
  - Token generation
  - Permission aggregation from roles

- [ ] Register in Dependency Injection
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => { ... });
  builder.Services.AddAuthorization();
  builder.Services.AddScoped<ITokenService, TokenService>();
  builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
  ```

- [ ] Update Program.cs middleware
  ```csharp
  app.UseAuthentication();
  app.UseAuthorization();
  app.UseMiddleware<TenantResolutionMiddleware>();
  ```

### Database Seeding
- [ ] Run EF Core migrations
  ```bash
  dotnet ef database update -p Src/Shared/Shared.Infrastructure
  ```

- [ ] Verify seed data files exist:
  - `SeedData/AuthData/admin.json`
  - `SeedData/AuthData/tenant.json`
  - `SeedData/AuthData/permissions.json`

- [ ] Test `InitialiseDatabaseAsync()` in startup

---

## Phase 2: Master Data APIs ✓

### Company Controller
- [ ] Create `CompanyController` in `Src/Product/Hrm.Api/Controllers/`
- [ ] Implement `GET /api/company/{id}` - Get company
- [ ] Implement `POST /api/company` - Create company [Authorize(Roles = "Admin")]
- [ ] Implement `PUT /api/company/{id}` - Update company [Authorize(Roles = "Admin")]
- [ ] Implement `DELETE /api/company/{id}` - Delete company [Authorize(Roles = "Admin")]

### Company Service
- [ ] Create `ICompanyService` interface
- [ ] Create `CompanyService` implementation
- [ ] Add DTO classes:
  - `CompanyDto`
  - `CreateCompanyRequest`
  - `UpdateCompanyRequest`

### Agent Controller
- [ ] Create `AgentController` in `Src/Product/Hrm.Api/Controllers/`
- [ ] Implement `GET /api/agent` - List agents with pagination
- [ ] Implement `GET /api/agent/{id}` - Get agent
- [ ] Implement `POST /api/agent` - Create agent [Authorize(Roles = "Admin,Manager")]
- [ ] Implement `PUT /api/agent/{id}` - Update agent [Authorize(Roles = "Admin,Manager")]
- [ ] Implement `DELETE /api/agent/{id}` - Delete agent [Authorize(Roles = "Admin")]

### Agent Service
- [ ] Create `IAgentService` interface
- [ ] Create `AgentService` implementation with CompanyId filtering
- [ ] Add DTO classes:
  - `AgentDto`
  - `CreateAgentRequest`
  - `UpdateAgentRequest`
  - `PagedResult<T>`

### Agent Role Controller
- [ ] Create `AgentRoleController`
- [ ] Implement `POST /api/agents/{agentId}/agent-role` - Assign role to agent
- [ ] Implement `GET /api/agents/{agentId}/agent-role` - Get agent roles
- [ ] Implement `DELETE /api/agents/{agentId}/agent-role/{roleId}` - Remove role

### Agent Role Service
- [ ] Create `IAgentRoleService` interface
- [ ] Create `AgentRoleService` implementation
- [ ] Add DTO classes:
  - `AgentRoleDto`
  - `AssignAgentRoleRequest`

---

## Phase 3: User Management ✓

### User Controller
- [ ] Create `UserController` in `Src/Product/Hrm.Api/Controllers/`
- [ ] Implement `POST /api/user/login` - User login [AllowAnonymous]
  - Accept: email, password
  - Return: JWT token + user profile
  - Log: successful/failed login attempts

- [ ] Implement `GET /api/user/profile` - Get current user profile [Authorize]
- [ ] Implement `POST /api/user` - Create user [Authorize(Roles = "Admin")]
- [ ] Implement `GET /api/user` - List users with pagination [Authorize(Roles = "Admin")]
- [ ] Implement `GET /api/user/{id}` - Get user details [Authorize]
- [ ] Implement `PUT /api/user/{id}` - Update user [Authorize]
- [ ] Implement `DELETE /api/user/{id}` - Delete user [Authorize(Roles = "Admin")]

### User Service
- [ ] Create `IUserService` interface with methods:
  - `LoginAsync(LoginRequest)`
  - `GetCurrentUserProfileAsync()`
  - `CreateUserAsync(CreateUserRequest)`
  - `GetUserAsync(Guid id)`
  - `GetUsersAsync(int page, int pageSize)`
  - `UpdateUserAsync(Guid id, UpdateUserRequest)`
  - `DeleteUserAsync(Guid id)`

- [ ] Create `UserService` implementation
  - Password hashing (bcrypt/PBKDF2)
  - Email uniqueness per company check
  - CompanyId assignment from tenant context
  - Soft delete support (ToDate)

- [ ] Add DTO classes:
  - `LoginRequest`
  - `CreateUserRequest`
  - `UpdateUserRequest`
  - `UserDto`
  - `AuthResponse`
  - `PagedResult<T>`

### User Role Assignment
- [ ] Create `UserRoleController`
- [ ] Implement `POST /api/users/{userId}/roles` - Assign role to user
- [ ] Implement `GET /api/users/{userId}/roles` - Get user roles
- [ ] Implement `DELETE /api/users/{userId}/roles/{roleId}` - Remove role

### User Role Service
- [ ] Create `IUserRoleService` interface
- [ ] Create `UserRoleService` implementation
- [ ] Add DTO classes:
  - `UserRoleDto`
  - `AssignRoleRequest`

### Login Logging
- [ ] Create `LoginLog` tracking
  - Track: UserId, Email, Timestamp, IP Address, Success/Failure, Reason
  - Store: In database for audit trail

---

## Phase 4: Role & Permission Management ✓

### Role Controller
- [ ] Create `RoleController`
- [ ] Implement `GET /api/role` - List all company roles [Authorize]
- [ ] Implement `GET /api/role/{id}` - Get role details [Authorize]
- [ ] Implement `POST /api/role` - Create role [Authorize(Roles = "Admin")]
- [ ] Implement `PUT /api/role/{id}` - Update role [Authorize(Roles = "Admin")]
- [ ] Implement `DELETE /api/role/{id}` - Delete role [Authorize(Roles = "Admin")]

### Role Service
- [ ] Create `IRoleService` interface
- [ ] Create `RoleService` implementation with CompanyId filtering
- [ ] Soft delete support (ToDate)
- [ ] Add DTO classes:
  - `RoleDto`
  - `CreateRoleRequest`
  - `UpdateRoleRequest`

### Permission Controller
- [ ] Create `PermissionController`
- [ ] Implement `GET /api/permission` - List permissions [Authorize]
- [ ] Implement `POST /api/permission` - Create permission [Authorize(Roles = "Admin")]

### Permission Service
- [ ] Create `IPermissionService` interface
- [ ] Create `PermissionService` implementation
- [ ] Add DTO classes:
  - `PermissionDto`
  - `CreatePermissionRequest`

### Role Permission Assignment
- [ ] Create `RolePermissionController`
- [ ] Implement `POST /api/roles/{roleId}/permissions` - Assign permission to role
- [ ] Implement `GET /api/roles/{roleId}/permissions` - Get role permissions
- [ ] Implement `DELETE /api/roles/{roleId}/permissions/{permissionId}` - Remove permission

### Role Permission Service
- [ ] Create `IRolePermissionService` interface
- [ ] Create `RolePermissionService` implementation
- [ ] Add DTO classes:
  - `RolePermissionDto`
  - `AssignPermissionRequest`

---

## Phase 5: Advanced Features ✓

### Module Controller
- [ ] Create `ModuleController`
- [ ] Implement `GET /api/module` - List available modules [Authorize]
- [ ] Implement `GET /api/module/{id}/permissions` - Get module permissions [Authorize]

### Module Service
- [ ] Create `IModuleService` interface
- [ ] Create `ModuleService` implementation
- [ ] Add DTO classes:
  - `ModuleDto`
  - `ModulePermissionDto`

### User Permissions Check
- [ ] Create `UserPermissionController`
- [ ] Implement `GET /api/users/me/permissions` - Get current user permissions [Authorize]
- [ ] Implement `POST /api/users/me/permissions/check` - Check specific permission [Authorize]

### User Permission Service
- [ ] Create `IUserPermissionService` interface
- [ ] Create `UserPermissionService` implementation
- [ ] Permission aggregation from user roles
- [ ] Add DTO classes:
  - `PermissionCheckRequest`
  - `PermissionCheckResponse`

### Agent User Assignment
- [ ] Create `AgentUserController`
- [ ] Implement `POST /api/agents/{agentId}/users` - Assign user to agent [Authorize(Roles = "Admin")]
- [ ] Implement `GET /api/agents/{agentId}/users` - Get agent users [Authorize]
- [ ] Implement `DELETE /api/agents/{agentId}/users/{userId}` - Remove user from agent [Authorize(Roles = "Admin")]

### Agent User Service
- [ ] Create `IAgentUserService` interface
- [ ] Create `AgentUserService` implementation
- [ ] Add DTO classes:
  - `AgentUserDto`
  - `AssignUserRequest`

---

## Cross-Cutting Concerns ✓

### Error Handling
- [ ] Create global exception handling middleware
  ```csharp
  app.UseMiddleware<ExceptionHandlingMiddleware>();
  ```

- [ ] Exception types:
  - `NotFoundException` (404)
  - `UnauthorizedException` (401)
  - `ForbiddenException` (403)
  - `ValidationException` (400)
  - `DuplicateException` (409)
  - `ConflictException` (409)

- [ ] Response wrapper class:
  ```csharp
  public class ApiResponse<T>
  {
      public bool Success { get; set; }
      public T Data { get; set; }
      public string Message { get; set; }
      public List<ValidationError> Errors { get; set; }
  }
  ```

### Validation
- [ ] Implement FluentValidation for all request DTOs
- [ ] Create validators:
  - `CreateCompanyValidator`
  - `CreateAgentValidator`
  - `CreateUserValidator`
  - `LoginRequestValidator`
  - etc.

- [ ] Register in DI:
  ```csharp
  builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
  ```

### Logging
- [ ] Setup Serilog
  ```csharp
  var logger = new LoggerConfiguration()
      .MinimumLevel.Information()
      .WriteTo.Console()
      .WriteTo.File("logs/hrm-.log", rollingInterval: RollingInterval.Day)
      .CreateLogger();

  builder.Host.UseSerilog(logger);
  ```

- [ ] Log important events:
  - User login (success/failure)
  - Data creation/update/deletion
  - Permission changes
  - Errors and exceptions

### CORS Configuration
- [ ] Configure CORS in Program.cs
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowFrontend", builder =>
      {
          builder.WithOrigins("https://yourdomain.com", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
      });
  });

  app.UseCors("AllowFrontend");
  ```

### AutoMapper Configuration
- [ ] Create mapping profiles
  - `CompanyMappingProfile`
  - `AgentMappingProfile`
  - `UserMappingProfile`
  - `RoleMappingProfile`
  - etc.

- [ ] Register in DI:
  ```csharp
  builder.Services.AddAutoMapper(typeof(Program));
  ```

---

## Documentation ✓

### Swagger/OpenAPI
- [ ] Configure Swagger in Program.cs
  ```csharp
  builder.Services.AddSwaggerGen(c =>
  {
      c.SwaggerDoc("v1", new() { Title = "HRM API", Version = "v1" });
      c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
  });
  ```

- [ ] Document all controllers with XML comments
- [ ] Add Swagger UI at startup

### API Endpoints Documentation
- [ ] Document each endpoint:
  - Purpose
  - Authorization requirements
  - Request parameters
  - Response format
  - Error codes
  - Example requests/responses

### Postman Collections
- [ ] Create collections for each phase
  - Authentication.postman_collection.json
  - CompanyManagement.postman_collection.json
  - AgentManagement.postman_collection.json
  - UserManagement.postman_collection.json
  - RolePermission.postman_collection.json

- [ ] Include:
  - Pre-request scripts (auth token setup)
  - Tests (response validation)
  - Example data
  - Environment variables

---

## Testing ✓

### Unit Tests
- [ ] Create unit test project: `Src/Tests/Hrm.Api.Tests`
- [ ] Test services:
  - CompanyService
  - AgentService
  - UserService
  - RoleService
  - PermissionService

- [ ] Test coverage targets: 80%+

### Integration Tests
- [ ] Test database operations
- [ ] Test multi-tenant isolation
- [ ] Test permission enforcement
- [ ] Test transaction handling

### API Tests
- [ ] Test endpoint authentication
- [ ] Test authorization
- [ ] Test validation
- [ ] Test error handling

---

## Security Checklist ✓

- [ ] JWT tokens validated on every request
- [ ] CompanyId verified in all database queries
- [ ] Passwords hashed using strong algorithm (bcrypt)
- [ ] HTTPS enforced in production
- [ ] CORS properly configured
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention (using EF Core)
- [ ] XSS prevention (API response, no HTML)
- [ ] CSRF tokens for state-changing operations
- [ ] Rate limiting configured
- [ ] Sensitive data not logged (passwords, tokens)
- [ ] API keys rotated regularly
- [ ] Database backups automated
- [ ] Audit logging for all user actions
- [ ] Error messages don't leak sensitive info

---

## Performance Checklist ✓

- [ ] Database indexes on frequently queried columns
- [ ] Query optimization (no N+1 queries)
- [ ] Pagination implemented for list endpoints
- [ ] Caching strategy defined
- [ ] Async/await used throughout
- [ ] Database connection pooling configured
- [ ] Entity Framework lazy loading disabled
- [ ] API response compression enabled
- [ ] Load testing completed
- [ ] CDN configured for static assets (if applicable)

---

## Deployment Checklist ✓

- [ ] Environment-specific configuration files
- [ ] Database migration scripts prepared
- [ ] CI/CD pipeline configured
- [ ] Health check endpoint implemented
- [ ] Monitoring and alerting setup
- [ ] Log aggregation configured
- [ ] Backup strategy documented
- [ ] Rollback procedure documented
- [ ] Load balancer configured
- [ ] SSL certificates configured

---

## Quick Start Commands

```bash
# Create migrations
dotnet ef migrations add InitialCreate -p Src/Shared/Shared.Infrastructure

# Apply migrations
dotnet ef database update -p Src/Shared/Shared.Infrastructure

# Run tests
dotnet test

# Run API
dotnet run --project Src/Product/Hrm.Api

# Generate API documentation
# (Swagger available at https://localhost:5001/swagger)
```

---

## Common Issues & Solutions

### Issue: CompanyId not being set automatically
**Solution**: Verify `TenantResolutionMiddleware` is registered and `StampTenant()` is called in `SaveChangesAsync()`

### Issue: Unauthorized errors on protected endpoints
**Solution**: Check JWT token is being sent in Authorization header and token includes CompanyId claim

### Issue: Cross-tenant data access
**Solution**: Verify all queries filter by `_tenantContext.CompanyId`

### Issue: Role permissions not being checked
**Solution**: Implement custom authorization policy handler for permission checks

---

## Next Steps After Implementation

1. **Performance Testing**: Load test with 1000+ concurrent users
2. **Security Audit**: Third-party security review
3. **Documentation**: API documentation for client developers
4. **Monitoring**: Setup APM (Application Performance Monitoring)
5. **Analytics**: Setup usage analytics and metrics
6. **Mobile API**: Consider mobile-specific endpoints if needed
7. **Webhooks**: Implement webhook system for real-time notifications
8. **Rate Limiting**: Implement per-user/per-company rate limits
9. **Caching**: Add Redis caching layer if needed
10. **Notification System**: Email/SMS notification system for important events

---

**Last Updated**: 2024  
**Status**: Ready for Implementation  
**Estimated Timeline**: 5 weeks
