# CoreSaaS — CurrentUserService vs ITenantContext
## Comparison, Decision, and Complete Fixed Implementation

---

## 1. Comparison

### What each approach actually does

| Concern | Your current project | ITenantContext approach |
|---|---|---|
| **Where CompanyId comes from** | `ICurrentUserService` reads JWT claim, handler uses it | `TenantResolutionMiddleware` reads JWT claim, stores in `ITenantContext`, DbContext reads it |
| **How queries are scoped** | Handler passes `companyId` explicitly to every `.Where(...)` | `HasQueryFilter` in `OnModelCreating` applies it invisibly to every EF query |
| **Who is responsible for isolation** | The developer writing the handler | EF Core, automatically |
| **Login / anonymous endpoints** | No problem — `companyId` comes from the request body | Problem — `ITenantContext` is empty before auth, must `IgnoreQueryFilters()` |
| **Middleware required?** | No | Yes — `TenantResolutionMiddleware` must run after `UseAuthentication` |
| **EF model cache bug risk?** | None | Yes — capturing a local variable in `HasQueryFilter` bakes in the first tenant's id for all future requests |
| **`UserManager<User>` compatibility** | Natural — Identity works as-is | Needs care — Identity internally calls `FindByEmailAsync` etc., which bypass your filter |
| **Complexity** | Low — one service, no middleware | Higher — middleware + accessor + context + DI trick |
| **Forgetting to filter** | A handler can accidentally omit `.Where(x => x.CompanyId == companyId)` and leak cross-tenant data | Impossible by default — must explicitly call `IgnoreQueryFilters()` |

### Actual bugs in your current code

| # | Location | Bug | Impact |
|---|---|---|---|
| 1 | `CurrentUserService.GetClaimValue` | `Claims.Single(...)` throws if claim is missing or duplicated; caught by a broad `catch` that silently returns `null` | A malformed token returns `null` on every property with no log, no trace |
| 2 | `AuditableEntity` | `public User? EntryBy` — base class holds a navigation to a domain entity in the same layer, coupling all auditable entities to `User` | Any entity using `AuditableEntity` drags in a full `User` graph; prevents reuse outside UserManagement |
| 3 | `CreateUserCommandHandler` | `await _mediator.Publish(evt)` is called **before** `await _unitOfWork.SaveChangesAsync()` | If `SaveChangesAsync` fails, the email is already sent and the reset URL points to a user that was never actually persisted |
| 4 | `UserRole : AuditableEntity` | `AuditableEntity` has `User? EntryBy` — `UserRole` is a junction entity that also inherits `EntryBy`, but junction creation never sets it | Always a `null` navigation with no enforcement |
| 5 | `ICurrentUserService` | `AgentId` and `BranchId` return hardcoded `null` | Handler immediately throws `UnauthorizedAccessException` when it reads `AgentId` — breaking every create-user call unless `AgentId` is read from elsewhere |

---

## 2. Decision: keep explicit filtering, fix the five bugs

Your project uses `UserManager<User>` for Identity, `Response<T>` as a standard envelope,
AutoMapper with nested `Profile` classes, and explicit `companyId` filtering everywhere.
Introducing `ITenantContext` on top of all that adds three moving parts
(middleware, accessor, DI trick) to solve a problem you've already solved, while creating
new ones (Identity's own queries bypass global filters, login needs special-casing).

**The right path is: keep explicit filtering, fix the bugs that are actually there.**

Fixes applied in this document:

- `Single` → `FirstOrDefault` in `GetClaimValue` (removes silent swallowing)
- `AuditableEntity` stores `Guid? EntryByUserId` scalar, not a `User?` navigation
- Domain event dispatch moves to **after** `SaveChangesAsync` (inside `UnitOfWork`)
- `AgentId` is read properly from JWT (or the handler receives it in the command)
- `ICurrentUserService` gets `IsAuthenticated` as a direct, testable property

---

## 3. Project structure (unchanged from your doc, shown for reference)

```
Src/
├── Shared/
│   ├── Shared.Domain/
│   │   ├── Abstractions/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── BaseEvent.cs
│   │   │   ├── IHasDomainEvents.cs
│   │   │   └── DomainEventCollection.cs
│   │   └── DTOs/
│   │       ├── UserInfo.cs
│   │       ├── Response.cs
│   │       └── ErrorDTO.cs
│   ├── Shared.Application/
│   │   └── Interface/
│   │       ├── ICurrentUserService.cs
│   │       ├── IUnitOfWork.cs
│   │       ├── IRepository.cs
│   │       └── IPasswordHasher.cs
│   └── Shared.Infrastructure/
│       ├── Service/
│       │   └── CurrentUserService.cs
│       └── Repositories/
│           ├── GenericRepository.cs
│           └── UnitOfWork.cs
└── UserManagement/
    ├── UserManagement.Domain/
    │   ├── Entities/
    │   │   ├── BaseEntities/
    │   │   │   └── AuditableEntity.cs
    │   │   ├── User.cs
    │   │   ├── Company.cs
    │   │   ├── Agent.cs
    │   │   ├── AgentUser.cs
    │   │   ├── AgentRole.cs
    │   │   ├── UserRole.cs
    │   │   ├── UserStatus.cs
    │   │   ├── Role.cs
    │   │   └── ModulePermission.cs
    │   └── Events/
    │       └── User/
    │           └── UserCreatedEvent.cs
    ├── UserManagement.Application/
    │   ├── Commands/UserCommands/CreateUser/
    │   │   ├── CreateUserCommand.cs
    │   │   └── CreateUserCommandHandler.cs
    │   └── ViewModels/
    │       ├── UserViewModel.cs
    │       └── RoleListViewModel.cs
    └── UserManagement.Infrastructure/
        ├── Persistence/
        │   ├── UserManagementDbContext.cs
        │   └── Configurations/
        │       └── UserConfiguration.cs
        └── DependencyInjection.cs
```

---

## 4. Shared.Domain

### `Abstractions/IHasDomainEvents.cs`
```csharp
// Shared.Domain/Abstractions/IHasDomainEvents.cs
namespace Shared.Domain.Abstractions;

public interface IHasDomainEvents
{
    IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

### `Abstractions/DomainEventCollection.cs`
```csharp
// Shared.Domain/Abstractions/DomainEventCollection.cs
namespace Shared.Domain.Abstractions;

// Extracted so both BaseEntity and User (which already inherits IdentityUser)
// can compose the same logic without duplicating it.
public sealed class DomainEventCollection
{
    private readonly List<BaseEvent> _events = [];
    public IReadOnlyCollection<BaseEvent> Events => _events.AsReadOnly();
    public void Add(BaseEvent domainEvent) => _events.Add(domainEvent);
    public void Clear() => _events.Clear();
}
```

### `Abstractions/BaseEvent.cs`
```csharp
// Shared.Domain/Abstractions/BaseEvent.cs
using MediatR;

namespace Shared.Domain.Abstractions;

public abstract class BaseEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

### `Abstractions/BaseEntity.cs`
```csharp
// Shared.Domain/Abstractions/BaseEntity.cs
namespace Shared.Domain.Abstractions;

public abstract class BaseEntity : IHasDomainEvents
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly DomainEventCollection _events = new();
    public IReadOnlyCollection<BaseEvent> DomainEvents => _events.Events;

    protected void AddDomainEvent(BaseEvent domainEvent) => _events.Add(domainEvent);
    public void ClearDomainEvents() => _events.Clear();
}
```

### `DTOs/UserInfo.cs`
```csharp
// Shared.Domain/DTOs/UserInfo.cs
namespace Shared.Domain.DTOs;

// CompanyId added — your updated CurrentUserService already populates it.
public class UserInfo(Guid id, string userName, string name, int companyId)
{
    public Guid Id { get; } = id;
    public string UserName { get; } = userName;
    public string Name { get; } = name;
    public int CompanyId { get; } = companyId;
}
```

### `DTOs/Response.cs`
```csharp
// Shared.Domain/DTOs/Response.cs
namespace Shared.Domain.DTOs;

public class Response<T> where T : class
{
    public bool Success { get; private set; }
    public string? Message { get; private set; }
    public List<ErrorDTO> Errors { get; private set; } = [];
    public T? Data { get; private set; }

    public static Response<T> SuccessResponse(T data, string? message = null) =>
        new() { Success = true, Message = message, Data = data };

    public static Response<T> SuccessResponse(string message) =>
        new() { Success = true, Message = message };

    public static Response<T> FailureResponse(params ErrorDTO[] errors) =>
        new() { Success = false, Errors = [.. errors] };
}
```

### `DTOs/ErrorDTO.cs`
```csharp
// Shared.Domain/DTOs/ErrorDTO.cs
namespace Shared.Domain.DTOs;

public record ErrorDTO(string Code, string? Message);
```

---

## 5. Shared.Application

### `Interface/ICurrentUserService.cs`
```csharp
// Shared.Application/Interface/ICurrentUserService.cs
using Shared.Domain.DTOs;

namespace Shared.Application.Interfaces;

// Kept exactly as your project declares it.
// AgentId is now properly resolved in the implementation instead of being null.
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }   // NEW — makes auth-checks in handlers testable
    Guid? UserId { get; }
    string? UserName { get; }
    UserInfo? UserInfo { get; }
    int? CompanyId { get; }
    Guid? AgentId { get; }          // now actually reads from claims
    Guid? BranchId { get; }
    Guid? CustomerId { get; }
}
```

### `Interface/IRepository.cs`
```csharp
// Shared.Application/Interface/IRepository.cs
namespace Shared.Application.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes);

    Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<List<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task InsertAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
```

### `Interface/IUnitOfWork.cs`
```csharp
// Shared.Application/Interface/IUnitOfWork.cs
namespace Shared.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;

    // FIX #3: SaveChangesAsync is the point where domain events are dispatched
    // (after commit). Callers don't call _mediator.Publish directly — UnitOfWork
    // handles it, so event dispatch can never accidentally happen before the save.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

---

## 6. Shared.Infrastructure

### `Service/CurrentUserService.cs`
```csharp
// Shared.Infrastructure/Service/CurrentUserService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application.Interfaces;
using Shared.Domain.DTOs;

namespace Shared.Infrastructure.Service;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal
        => _httpContextAccessor.HttpContext?.User;

    // FIX #1 (primary fix): IsAuthenticated is now a direct, readable property.
    // Previously this check was buried inside the private GetClaimValue, making
    // it impossible to test or read in a handler without calling GetClaimValue.
    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
        => Guid.TryParse(GetClaimValue("UserId"), out var id) ? id : null;

    public string? UserName
        => GetClaimValue("UserName");

    public int? CompanyId
        => int.TryParse(GetClaimValue("CompanyId"), out var id) ? id : null;

    // FIX #5: Actually reads from claims instead of returning null.
    // Your token generation must include an "AgentId" claim for this to work.
    // If the token does not carry AgentId, change this to read it from the DB
    // inside the handler (see CreateUserCommandHandler, step 4b below).
    public Guid? AgentId
        => Guid.TryParse(GetClaimValue("AgentId"), out var id) ? id : null;

    public Guid? BranchId
        => Guid.TryParse(GetClaimValue("BranchId"), out var id) ? id : null;

    public Guid? CustomerId
        => Guid.TryParse(GetClaimValue("CustomerId"), out var id) ? id : null;

    public UserInfo? UserInfo
    {
        get
        {
            if (!IsAuthenticated || !UserId.HasValue || string.IsNullOrEmpty(UserName))
                return null;

            return new UserInfo(
                UserId.Value,
                UserName,
                GetClaimValue("Name") ?? UserName,
                CompanyId ?? 0);
        }
    }

    // FIX #1 (the actual change): FirstOrDefault instead of Single.
    //
    // Single() throws InvalidOperationException when:
    //   - the claim is missing (sequence contains no elements)
    //   - the claim appears more than once (sequence contains more than one element)
    //
    // Both exceptions were silently swallowed by the outer catch, returning null
    // with zero trace of what went wrong. A duplicate CompanyId claim (easy to
    // produce by accidentally adding it twice in token generation) would silently
    // return null for CompanyId, causing every handler to throw
    // UnauthorizedAccessException with no useful error message.
    //
    // FirstOrDefault returns null when not found, and takes the first when
    // duplicated — no exception, no catch block needed at all.
    private string? GetClaimValue(string claimType)
    {
        if (!IsAuthenticated) return null;
        return Principal?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
```

### `Repositories/GenericRepository.cs`
```csharp
// Shared.Infrastructure/Repositories/GenericRepository.cs
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> GetAll(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        return query;
    }

    public async Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return await query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<List<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
```

### `Repositories/UnitOfWork.cs`
```csharp
// Shared.Infrastructure/Repositories/UnitOfWork.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly IPublisher _publisher;
    private readonly Dictionary<Type, object> _repositories = [];

    public UnitOfWork(DbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
            _repositories[type] = new GenericRepository<T>(_context);
        return (IRepository<T>)_repositories[type];
    }

    // FIX #3: snapshot events BEFORE saving, dispatch AFTER.
    //
    // Your original handler did:
    //   await _mediator.Publish(evt);          ← email sent here
    //   await _unitOfWork.SaveChangesAsync();  ← DB save here
    //
    // If SaveChangesAsync throws (DB down, constraint violation, anything),
    // the email has already gone out pointing to a user that was never saved.
    // The recipient clicks "set password", the token lookup fails, they're stuck.
    //
    // Post-commit dispatch means: if the DB save fails, no event is published.
    // If publishing fails, the user record is already safe in the DB and you
    // can replay the event later.
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = _context.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        // Commit first — identity-column keys (int) and DB-generated values are
        // now populated on the entity instances.
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch after commit. Each handler sees the correct, persisted state.
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }

    public void Dispose() => _context.Dispose();
}
```

---

## 7. UserManagement.Domain

### `Entities/BaseEntities/AuditableEntity.cs`
```csharp
// UserManagement.Domain/Entities/BaseEntities/AuditableEntity.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities.BaseEntities;

// FIX #2: removed "public User? EntryBy" navigation property.
//
// Original design had:
//   public User? EntryBy { get; private set; }
//   public void SetEntry(User? entryBy) { EntryBy = entryBy; ... }
//
// A base class that holds a navigation to a concrete entity (User) that lives
// at the same domain level means: every entity that inherits AuditableEntity
// (Agent, UserRole, AgentRole, ...) carries a full User graph.
// More importantly, it creates a conceptual circular dependency:
//   AuditableEntity knows about User
//   User (which is also an entity in this domain) implicitly "is-a" AuditableEntity
//   via its own auditing approach — the two concepts are tangled.
//
// The fix: store the FK scalar. The EF configuration (UserConfiguration) maps
// the shadow foreign key "EntryByUserId" to the correct column. Handlers that
// need to show "who created this" join through the scalar, they don't navigate.
public abstract class AuditableEntity : BaseEntity
{
    public int CompanyId { get; set; }

    public Guid? EntryByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    public VerificationStatus VerificationStatus { get; private set; }
        = VerificationStatus.Saved;

    public void SetEntry(Guid? entryByUserId)
    {
        EntryByUserId = entryByUserId;
        UpdatedDate = DateTime.UtcNow;
    }

    public void SetUpdate(Guid? updatedByUserId)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Submit() => VerificationStatus = VerificationStatus.Submitted;
    public void Approve() => VerificationStatus = VerificationStatus.Approved;
    public void Reject() => VerificationStatus = VerificationStatus.Rejected;

    protected void SetTerminationDate(DateTime? date = null)
        => ToDate = date ?? DateTime.UtcNow;

    public bool IsTerminated => ToDate is not null;
    public bool IsVerified => VerificationStatus == VerificationStatus.Approved;
    public bool IsRejected => VerificationStatus == VerificationStatus.Rejected;

    public bool ValidOnDate(DateOnly date)
    {
        if (!IsVerified) return false;
        return date >= DateOnly.FromDateTime(EntryDate)
            && (ToDate is null || date <= DateOnly.FromDateTime(ToDate.Value));
    }
}

public enum VerificationStatus { Saved, Submitted, Approved, Rejected }
```

### `Entities/Company.cs`
```csharp
// UserManagement.Domain/Entities/Company.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class Company : BaseEntity
{
    // int Id for backward-compat with your existing schema — override the Guid
    // from BaseEntity using new keyword deliberately, documented.
    public new int Id { get; private set; }
    public string? ProductCode { get; private set; }
    public string? Name { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNo { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public string? Url { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Agent> _agents = [];
    public IReadOnlyCollection<Agent> Agents => _agents.AsReadOnly();

    public Company(string name, string email, string address, string phoneNo,
                   string pan, string regNo, string url, string productCode = "HRM")
    {
        Name = name;
        Email = email;
        Address = address;
        PhoneNo = phoneNo;
        Pan = pan;
        RegNo = regNo;
        Url = url;
        ProductCode = productCode;
    }

    private Company() { }
}
```

### `Entities/User.cs`
```csharp
// UserManagement.Domain/Entities/User.cs
using Microsoft.AspNetCore.Identity;
using Shared.Domain.Abstractions;
using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

// User inherits IdentityUser<Guid> so it cannot also inherit BaseEntity.
// IHasDomainEvents + DomainEventCollection gives it the same event capability
// by composition rather than inheritance — no code duplication.
public class User : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly DomainEventCollection _events = new();
    public IReadOnlyCollection<BaseEvent> DomainEvents => _events.Events;
    public void ClearDomainEvents() => _events.Clear();

    // Tenant isolation — explicit, always set from Company.Id
    public int CompanyId { get; private set; }
    public Company? Company { get; private set; }

    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? LastName { get; private set; }

    public string? FullName =>
        string.Join(" ", new[] { FirstName, MiddleName, LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    public string? Contact { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // FIX #2 result: audit stored as scalar FK, not a navigation.
    // The EF configuration will map this to a column and FK constraint.
    public Guid? EntryByUserId { get; private set; }
    public DateTime EntryDate { get; private set; } = DateTime.UtcNow;

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserStatus> _userStatuses = [];
    public IReadOnlyCollection<UserStatus> UserStatuses => _userStatuses.AsReadOnly();

    private readonly List<AgentUser> _agentUsers = [];
    public IReadOnlyCollection<AgentUser> AgentUsers => _agentUsers.AsReadOnly();

    private readonly List<UserModulePermission> _userModulePermissions = [];
    public IReadOnlyCollection<UserModulePermission> UserModulePermissions
        => _userModulePermissions.AsReadOnly();

    private User() { }

    public User(Company company, string userName, string firstName, string? middleName,
                string lastName, string email, string? contact, Guid? entryByUserId)
    {
        Id = Guid.NewGuid();
        Company = company;
        CompanyId = company.Id;
        UserName = userName;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        Contact = contact;
        EntryByUserId = entryByUserId;
        EntryDate = DateTime.UtcNow;
        _userStatuses.Add(new UserStatus(Id, remarks: null));
    }

    public void AddRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id && ur.ToDate is null)) return;
        _userRoles.Add(new UserRole(Id, role));
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.SingleOrDefault(ur => ur.RoleId == roleId && ur.ToDate is null);
        userRole?.Terminate();
    }

    public void AddToAgent(Agent agent)
    {
        // Terminate any existing agent assignment before adding a new one
        _agentUsers.Where(au => au.ToDate is null).ToList().ForEach(au => au.Terminate());
        _agentUsers.Add(new AgentUser(Id, agent.Id));
    }

    public void AddModulePermission(ModulePermission modulePermission)
        => _userModulePermissions.Add(new UserModulePermission(Id, modulePermission.Id));

    public void RemoveModulePermission(Guid modulePermissionId)
    {
        var perm = _userModulePermissions
            .SingleOrDefault(p => p.ModulePermissionId == modulePermissionId);
        if (perm is not null) _userModulePermissions.Remove(perm);
    }

    public void Update(string userName, string firstName, string? middleName,
                       string lastName, string email, string? contact, Guid? updatedByUserId)
    {
        UserName = userName;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        Contact = contact;
        EntryByUserId = updatedByUserId;
    }
}
```

### `Entities/UserRole.cs`
```csharp
// UserManagement.Domain/Entities/UserRole.cs
using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class UserRole : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Role role)
    {
        UserId = userId;
        RoleId = role.Id;
        Role = role;
        SetEntry(userId); // FIX #4: junction entity sets its own audit on creation
    }

    public void Terminate() => SetTerminationDate();
}
```

### `Entities/UserStatus.cs`
```csharp
// UserManagement.Domain/Entities/UserStatus.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class UserStatus : BaseEntity
{
    public Guid UserId { get; private set; }
    public string? Remarks { get; private set; }
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }

    private UserStatus() { }

    public UserStatus(Guid userId, string? remarks)
    {
        UserId = userId;
        Remarks = remarks;
    }

    public void Terminate(string? remarks)
    {
        ToDate = DateTime.UtcNow;
        Remarks = remarks;
    }
}
```

### `Entities/AgentUser.cs`
```csharp
// UserManagement.Domain/Entities/AgentUser.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class AgentUser : BaseEntity
{
    public Guid UserId { get; private set; }
    public int AgentId { get; private set; }
    public Agent? Agent { get; private set; }
    public DateTime FromDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ToDate { get; private set; }

    private AgentUser() { }

    public AgentUser(Guid userId, int agentId)
    {
        UserId = userId;
        AgentId = agentId;
    }

    public void Terminate() => ToDate = DateTime.UtcNow;
}
```

### `Entities/Agent.cs`
```csharp
// UserManagement.Domain/Entities/Agent.cs
using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class Agent : AuditableEntity
{
    public string? Name { get; private set; }
    public string? Address { get; private set; }
    public string? Pan { get; private set; }
    public string? RegNo { get; private set; }
    public bool IsParent { get; private set; }
    public string? ReferralCode { get; private set; }

    private readonly List<AgentUser> _agentUsers = [];
    public IReadOnlyCollection<AgentUser> AgentUsers => _agentUsers.AsReadOnly();

    private readonly List<AgentRole> _agentRoles = [];
    public IReadOnlyCollection<AgentRole> AgentRoles => _agentRoles.AsReadOnly();

    private Agent() { }

    public Agent(string name, string address, string pan, string regNo,
                 bool isParent, int companyId, Guid? entryByUserId = null)
    {
        Name = name;
        Address = address;
        Pan = pan;
        RegNo = regNo;
        IsParent = isParent;
        CompanyId = companyId;
        ReferralCode = GenerateReferralCode(name);
        SetEntry(entryByUserId);
    }

    private static string GenerateReferralCode(string name)
        => $"REF_{name.ToUpper()[..Math.Min(4, name.Length)]}_{Guid.NewGuid().ToString()[..8]}";
}
```

### `Entities/Role.cs`
```csharp
// UserManagement.Domain/Entities/Role.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class Role : BaseEntity
{
    public int CompanyId { get; private set; }
    public string? Name { get; private set; }
    public string? Desc { get; private set; }

    private Role() { }

    public Role(string name, string? desc, int companyId)
    {
        Name = name;
        Desc = desc;
        CompanyId = companyId;
    }
}
```

### `Entities/AgentRole.cs`
```csharp
// UserManagement.Domain/Entities/AgentRole.cs
using UserManagement.Domain.Entities.BaseEntities;

namespace UserManagement.Domain.Entities;

public class AgentRole : AuditableEntity
{
    public int AgentId { get; private set; }
    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }

    private AgentRole() { }

    public AgentRole(int agentId, Role role, int companyId)
    {
        AgentId = agentId;
        RoleId = role.Id;
        Role = role;
        CompanyId = companyId;
    }
}
```

### `Entities/ModulePermission.cs`
```csharp
// UserManagement.Domain/Entities/ModulePermission.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class ModulePermission : BaseEntity
{
    public int CompanyId { get; private set; }
    public string? Code { get; private set; }
    public string? Description { get; private set; }

    private ModulePermission() { }
}
```

### `Entities/UserModulePermission.cs`
```csharp
// UserManagement.Domain/Entities/UserModulePermission.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Entities;

public class UserModulePermission : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ModulePermissionId { get; private set; }
    public ModulePermission? ModulePermission { get; private set; }

    private UserModulePermission() { }

    public UserModulePermission(Guid userId, Guid modulePermissionId)
    {
        UserId = userId;
        ModulePermissionId = modulePermissionId;
    }
}
```

### `Events/User/UserCreatedEvent.cs`
```csharp
// UserManagement.Domain/Events/User/UserCreatedEvent.cs
using Shared.Domain.Abstractions;

namespace UserManagement.Domain.Events.User;

public class UserCreatedEvent : BaseEvent
{
    public string? FullName { get; }
    public string? UserName { get; }
    public string? Email { get; }
    public string? ResetPasswordUrl { get; }

    public UserCreatedEvent(string? fullName, string? userName, string? email, string? resetPasswordUrl)
    {
        FullName = fullName;
        UserName = userName;
        Email = email;
        ResetPasswordUrl = resetPasswordUrl;
    }
}
```

---

## 8. UserManagement.Application

### `Commands/UserCommands/CreateUser/CreateUserCommand.cs`
```csharp
// UserManagement.Application/Commands/UserCommands/CreateUser/CreateUserCommand.cs
using MediatR;
using Shared.Domain.DTOs;
using UserManagement.Application.ViewModels;

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

### `Commands/UserCommands/CreateUser/CreateUserCommandHandler.cs`
```csharp
// UserManagement.Application/Commands/UserCommands/CreateUser/CreateUserCommandHandler.cs
using System.Web;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Application.Interfaces;
using Shared.Domain.DTOs;
using UserManagement.Application.ViewModels;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events.User;

namespace UserManagement.Application.Commands.UserCommands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly MailConfig _mailConfig;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _mailConfig = mailConfig;
    }

    public async Task<Response<UserViewModel>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Read caller identity ─────────────────────────────────────────
        var userInfo = _currentUserService.UserInfo
            ?? return Response<UserViewModel>.FailureResponse(Errors.Unauthorized);

        var companyId = userInfo.CompanyId;
        var callerUserId = userInfo.Id;

        // FIX #5: AgentId now comes from claims (see CurrentUserService).
        // If your token doesn't carry AgentId, add it at token-generation time,
        // or add an AgentId parameter to the command and let the controller supply it.
        var agentId = _currentUserService.AgentId
            ?? return Response<UserViewModel>.FailureResponse(Errors.Unauthorized);

        // ── 2. Validate inputs ──────────────────────────────────────────────
        if (request.Roles.Count == 0)
            return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

        // ── 3. Fetch repositories ───────────────────────────────────────────
        var companyRepo = _unitOfWork.Repository<Company>();
        var agentRepo   = _unitOfWork.Repository<Agent>();
        var agentRoleRepo = _unitOfWork.Repository<AgentRole>();
        var modulePermissionRepo = _unitOfWork.Repository<ModulePermission>();

        // ── 4. Fetch related domain objects (all explicit companyId scoping) ─
        var company = await companyRepo.GetSingleOrDefaultAsync(
            x => x.Id == companyId, cancellationToken);

        if (company is null)
            return Response<UserViewModel>.FailureResponse(Errors.CompanyNotFound);

        var agent = await agentRepo.GetSingleOrDefaultAsync(
            x => x.Id == agentId && x.CompanyId == companyId, cancellationToken);

        if (agent is null)
            return Response<UserViewModel>.FailureResponse(Errors.AgentNotFound);

        // ── 5. Build domain entity ──────────────────────────────────────────
        // FIX #2 result: pass callerUserId (Guid) not a User navigation object.
        var user = new User(
            company,
            request.UserName,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Email,
            request.Contact,
            entryByUserId: callerUserId);

        user.AddToAgent(agent);

        // ── 6. Assign roles ─────────────────────────────────────────────────
        var agentRoles = agentRoleRepo
            .GetAll(
                x => request.Roles.Contains(x.Id) && x.CompanyId == companyId,
                includes: x => x.Role!)
            .ToList();

        if (agentRoles.Count == 0)
            return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

        agentRoles.ForEach(ar => user.AddRole(ar.Role!));

        // ── 7. Assign module permissions ────────────────────────────────────
        var permissions = await modulePermissionRepo.GetListAsync(
            x => request.ModulePermissions.Contains(x.Id) && x.CompanyId == companyId,
            cancellationToken: cancellationToken);

        permissions.ForEach(user.AddModulePermission);

        // ── 8. Persist via Identity ─────────────────────────────────────────
        // UserManager.CreateAsync saves through its own DbContext path.
        // We call SaveChangesAsync on our UnitOfWork next to ensure the
        // child collections (UserRoles, AgentUsers, etc.) are also persisted.
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            return Response<UserViewModel>.FailureResponse(
                identityResult.Errors
                    .Select(e => new ErrorDTO(e.Code, e.Description))
                    .ToArray());
        }

        // ── 9. Prepare domain event ─────────────────────────────────────────
        // FIX #3: the event is ADDED to the entity here (not published yet).
        // UnitOfWork.SaveChangesAsync will dispatch it AFTER the DB commit.
        // Previously: _mediator.Publish(evt) ran before SaveChangesAsync,
        // so email could be sent for a user that was never actually saved.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = _mailConfig.OfficeURL
            + string.Format(_mailConfig.OfficeNewUserUrl,
                HttpUtility.UrlEncode(token),
                HttpUtility.UrlEncode(request.Email));

        // We can't call AddDomainEvent directly (User doesn't inherit BaseEntity),
        // but UserCreatedEvent is a standalone event — publish it via UnitOfWork
        // by attaching it to a wrapper, or publish after SaveChangesAsync manually.
        // Simplest correct approach for User (which inherits IdentityUser, not BaseEntity):
        // save first, then publish the one notification that has external side effects.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 10. Publish AFTER save ──────────────────────────────────────────
        // This is the manual post-commit publish for User-specific events.
        // Domain events attached to entities that inherit BaseEntity are
        // dispatched automatically inside UnitOfWork. For User (which inherits
        // IdentityUser instead), we dispatch explicitly here — same order guarantee.
        var createdEvent = new UserCreatedEvent(user.FullName, user.UserName, user.Email, resetUrl);

        // Use a dedicated publisher field if you prefer, or extend UnitOfWork.
        // Injecting IPublisher here keeps the handler self-contained.

        // ── 11. Map to ViewModel ────────────────────────────────────────────
        var viewModel = _mapper.Map<UserViewModel>(user);

        // Remap role IDs to AgentRole IDs (your original requirement preserved)
        foreach (var role in viewModel.RoleList)
            role.Id = agentRoles.Single(ar => ar.Role!.Id == role.Id).Id;

        return Response<UserViewModel>.SuccessResponse(viewModel);
    }
}
```

> **Note on step 10:** Because `User` inherits `IdentityUser<Guid>` and not `BaseEntity`,
> it cannot use `AddDomainEvent` automatically. Inject `IPublisher` (from MediatR) into
> the handler and call `await _publisher.Publish(createdEvent, cancellationToken)` after
> `SaveChangesAsync`. This preserves the "email only goes out after DB is committed"
> guarantee from FIX #3.

### `Commands/UserCommands/CreateUser/CreateUserCommandHandler.cs` (complete with IPublisher)
```csharp
// Full version with IPublisher injected for post-commit event dispatch on User

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserViewModel>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly IPublisher _publisher;       // ← added
    private readonly MailConfig _mailConfig;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService,
        UserManager<User> userManager, IPublisher publisher, MailConfig mailConfig)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _publisher = publisher;
        _mailConfig = mailConfig;
    }

    public async Task<Response<UserViewModel>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userInfo  = _currentUserService.UserInfo;
        var agentId   = _currentUserService.AgentId;

        if (userInfo is null || agentId is null)
            return Response<UserViewModel>.FailureResponse(Errors.Unauthorized);

        var companyId    = userInfo.CompanyId;
        var callerUserId = userInfo.Id;

        if (request.Roles.Count == 0)
            return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

        var company = await _unitOfWork.Repository<Company>()
            .GetSingleOrDefaultAsync(x => x.Id == companyId, cancellationToken);
        if (company is null)
            return Response<UserViewModel>.FailureResponse(Errors.CompanyNotFound);

        var agent = await _unitOfWork.Repository<Agent>()
            .GetSingleOrDefaultAsync(x => x.Id == agentId && x.CompanyId == companyId, cancellationToken);
        if (agent is null)
            return Response<UserViewModel>.FailureResponse(Errors.AgentNotFound);

        var user = new User(company, request.UserName, request.FirstName, request.MiddleName,
                            request.LastName, request.Email, request.Contact, callerUserId);

        user.AddToAgent(agent);

        var agentRoles = _unitOfWork.Repository<AgentRole>()
            .GetAll(x => request.Roles.Contains(x.Id) && x.CompanyId == companyId,
                    includes: x => x.Role!)
            .ToList();

        if (agentRoles.Count == 0)
            return Response<UserViewModel>.FailureResponse(Errors.RoleIsRequired);

        agentRoles.ForEach(ar => user.AddRole(ar.Role!));

        var permissions = await _unitOfWork.Repository<ModulePermission>()
            .GetListAsync(x => request.ModulePermissions.Contains(x.Id) && x.CompanyId == companyId,
                          cancellationToken: cancellationToken);
        permissions.ForEach(user.AddModulePermission);

        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
            return Response<UserViewModel>.FailureResponse(
                identityResult.Errors.Select(e => new ErrorDTO(e.Code, e.Description)).ToArray());

        // Save child collections (UserRoles, AgentUsers, UserModulePermissions)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Post-commit: now the user is safely in the DB, generate the token ──
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = _mailConfig.OfficeURL
            + string.Format(_mailConfig.OfficeNewUserUrl,
                HttpUtility.UrlEncode(token), HttpUtility.UrlEncode(request.Email));

        // Dispatch AFTER save — email goes out only for persisted users
        await _publisher.Publish(
            new UserCreatedEvent(user.FullName, user.UserName, user.Email, resetUrl),
            cancellationToken);

        var viewModel = _mapper.Map<UserViewModel>(user);
        foreach (var role in viewModel.RoleList)
            role.Id = agentRoles.Single(ar => ar.Role!.Id == role.Id).Id;

        return Response<UserViewModel>.SuccessResponse(viewModel);
    }
}
```

### `ViewModels/UserViewModel.cs`
```csharp
// UserManagement.Application/ViewModels/UserViewModel.cs
using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.ViewModels;

public class UserViewModel
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public Guid AgentId { get; set; }
    public string? AgentName { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<RoleListViewModel> RoleList { get; set; } = [];
    public List<ModulePermissionViewModel> UserModulePermissionList { get; set; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(d => d.AgentId,
                    o => o.MapFrom(s => s.AgentUsers
                        .Where(au => au.ToDate == null)
                        .Select(au => au.Agent!.Id)
                        .FirstOrDefault()))
                .ForMember(d => d.AgentName,
                    o => o.MapFrom(s => s.AgentUsers
                        .Where(au => au.ToDate == null)
                        .Select(au => au.Agent!.Name)
                        .FirstOrDefault()))
                .ForMember(d => d.RoleList,
                    o => o.MapFrom(s => s.UserRoles.Where(ur => ur.ToDate == null)))
                .ForMember(d => d.UserModulePermissionList,
                    o => o.MapFrom(s => s.UserModulePermissions));
        }
    }
}
```

### `ViewModels/RoleListViewModel.cs`
```csharp
// UserManagement.Application/ViewModels/RoleListViewModel.cs
using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.ViewModels;

public class RoleListViewModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Desc { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Role, RoleListViewModel>();

            CreateMap<AgentRole, RoleListViewModel>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Role!.Name))
                .ForMember(d => d.Desc, o => o.MapFrom(s => s.Role!.Desc));

            CreateMap<UserRole, RoleListViewModel>()
                .ForMember(d => d.Id,   o => o.MapFrom(s => s.Role!.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Role!.Name))
                .ForMember(d => d.Desc, o => o.MapFrom(s => s.Role!.Desc));
        }
    }
}
```

### `ViewModels/ModulePermissionViewModel.cs`
```csharp
// UserManagement.Application/ViewModels/ModulePermissionViewModel.cs
using AutoMapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.ViewModels;

public class ModulePermissionViewModel
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserModulePermission, ModulePermissionViewModel>()
                .ForMember(d => d.Id,          o => o.MapFrom(s => s.ModulePermission!.Id))
                .ForMember(d => d.Code,        o => o.MapFrom(s => s.ModulePermission!.Code))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.ModulePermission!.Description));
        }
    }
}
```

### `Errors/Errors.cs`
```csharp
// UserManagement.Application/Errors/Errors.cs
using Shared.Domain.DTOs;

namespace UserManagement.Application;

public static class Errors
{
    public static ErrorDTO Unauthorized      => new("UNAUTHORIZED",    "Authentication required.");
    public static ErrorDTO RoleIsRequired    => new("ROLE_REQUIRED",   "At least one role must be assigned.");
    public static ErrorDTO CompanyNotFound   => new("COMPANY_NOT_FOUND","Company not found.");
    public static ErrorDTO AgentNotFound     => new("AGENT_NOT_FOUND", "Agent not found for this company.");
    public static ErrorDTO Exception(Exception ex) => new("EXCEPTION", ex.Message);
}
```

---

## 9. UserManagement.Infrastructure

### `Persistence/Configurations/UserConfiguration.cs`
```csharp
// UserManagement.Infrastructure/Persistence/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UserId");

        builder.Property(u => u.FirstName).HasMaxLength(30);
        builder.Property(u => u.MiddleName).HasMaxLength(30);
        builder.Property(u => u.LastName).HasMaxLength(30);
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.Contact).HasMaxLength(256);
        builder.Property(u => u.CompanyId).IsRequired();

        // FIX #2 result: maps EntryByUserId scalar to the FK column.
        // No navigation to User in AuditableEntity means no circular EF graph.
        builder.Property(u => u.EntryByUserId).HasColumnName("EntryByUserId");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.EntryByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.CompanyId, u.NormalizedEmail }).IsUnique();

        // Field-access collections (private backing lists)
        builder.Navigation(u => u.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserStatuses).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.AgentUsers).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.UserModulePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

### `Persistence/UserManagementDbContext.cs`
```csharp
// UserManagement.Infrastructure/Persistence/UserManagementDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence;

// IdentityDbContext wires up the Identity tables automatically.
// No ITenantContext injection here — tenant filtering is done explicitly by each handler.
public class UserManagementDbContext : IdentityDbContext<User, Role, Guid>
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
        : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentUser> AgentUsers => Set<AgentUser>();
    public DbSet<AgentRole> AgentRoles => Set<AgentRole>();
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();
    public DbSet<ModulePermission> ModulePermissions => Set<ModulePermission>();
    public DbSet<UserModulePermission> UserModulePermissions => Set<UserModulePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UserManagementDbContext).Assembly);
    }
}
```

### `DependencyInjection.cs`
```csharp
// UserManagement.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<UserManagementDbContext>(
            options => options.UseSqlServer(connectionString));

        // Register as the generic DbContext so UnitOfWork resolves it.
        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<UserManagementDbContext>());

        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<UserManagementDbContext>();

        return services;
    }
}
```

---

## 10. Shared.Infrastructure — `DependencyInjection.cs`

```csharp
// Shared.Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.Service;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
```

---

## 11. Host — `Program.cs`

```csharp
// UserManagement.Api/Program.cs  (or Hrm.Api / CoreSaaS.Api)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure;
using System.Text;
using UserManagement.Application;
using UserManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Shared infrastructure (CurrentUserService, UnitOfWork)
builder.Services.AddSharedInfrastructure();

// 2. UserManagement infrastructure (DbContext, Identity)
builder.Services.AddUserManagementInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// 3. Application layer (MediatR handlers, AutoMapper)
builder.Services.AddUserManagementApplication();

// 4. MailConfig (bound from appsettings)
builder.Services.AddSingleton(
    builder.Configuration.GetSection("MailConfig").Get<MailConfig>()!);

// 5. JWT auth — no middleware changes needed (no ITenantContext to feed)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
// NOTE: no TenantResolutionMiddleware — it has nothing to do here.
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### `UserManagement.Application/DependencyInjection.cs`
```csharp
// UserManagement.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

---

## 12. All five fixes in one place

| Fix | Where | Before | After |
|---|---|---|---|
| **#1** `Single→FirstOrDefault` | `CurrentUserService.GetClaimValue` | `Claims.Single(c => c.Type == claimType).Value` inside a `try/catch` that swallows every exception silently | `Claims.FirstOrDefault(c => c.Type == claimType)?.Value` — no exception, no catch needed |
| **#2** `AuditableEntity.EntryBy` | `AuditableEntity` | `public User? EntryBy` navigation property — base class coupled to a domain entity | `public Guid? EntryByUserId` scalar FK — base class knows nothing about `User` |
| **#3** Event before save | `CreateUserCommandHandler` | `_mediator.Publish(evt)` then `SaveChangesAsync()` — email sent before DB committed | `SaveChangesAsync()` then `_publisher.Publish(evt)` — email only goes out after user is persisted |
| **#4** Junction entity audit | `UserRole` | Inherits `AuditableEntity.EntryBy` but never sets it — always `null` | `SetEntry(userId)` called in constructor using the scalar setter |
| **#5** `AgentId` always null | `CurrentUserService.AgentId` | `=> null` — handler immediately threw `UnauthorizedAccessException` | Reads `"AgentId"` claim: `Guid.TryParse(GetClaimValue("AgentId"), out var id) ? id : null` |
