
using Microsoft.AspNetCore.Identity;
using Shared.Domain.Abstractions;
using Security.Domain.Entities;

namespace UserManagement.Domain.Entities;

// User inherits IdentityUser<Guid> so it cannot also inherit BaseEntity.
// IHasDomainEvents + DomainEventCollection gives it the same event capability
// by composition rather than inheritance — no code duplication.
public class User : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly DomainEventCollection _events = new();
    public IReadOnlyCollection<BaseEvent> DomainEvents => _events.Events;
    public void ClearDomainEvents() => _events.Clear();

    // Tenant isolation — explicit CompanyId always set from Company.Id
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

    // FIX #2: Audit stored as scalar FK, not a navigation
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
        _userStatuses.Add(new UserStatus(remarks: null));
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
