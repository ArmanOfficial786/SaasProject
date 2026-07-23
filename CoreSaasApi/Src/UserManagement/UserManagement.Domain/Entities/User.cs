
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace UserManagement.Domain.Entities;

// User inherits IdentityUser<Guid> so it cannot also inherit BaseEntity.
// IHasDomainEvents is implemented directly here, by composition, mirroring
// exactly what BaseEntity does — including the [NotMapped] guard, which is
// the piece that was missing before and caused EF to try to map BaseEvent
// as a real entity type.
public class User : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<BaseEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(BaseEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    // null for super admin users, otherwise set to the specific company ID
    public int? CompanyId { get; private set; }
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

    public User(string userName, string firstName, string? middleName,
                string lastName, string email, string? contact, Guid? entryByUserId, int? companyId = null)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
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
        var targetRole = CompanyId ?? role.CompanyId;
        if (role.IsCompanyRole)
        {
            if (role.CompanyId != targetRole)
                throw new InvalidOperationException("Cannot assign a role from a different company.");

            if (_userRoles.Any(ur => ur.RoleId == role.Id && ur.CompanyId == targetRole && ur.ToDate is null)) return;
            _userRoles.Add(new UserRole(Id, role, CompanyId));
        }
        else if (role.IsGlobalRole)
        {
            if (_userRoles.Any(ur => ur.RoleId == role.Id && ur.ToDate is null)) return;
            _userRoles.Add(new UserRole(Id, role));
        }

    }

    public void RemoveRole(Guid roleId, int? companyId = null)
    {
        var targetCompanyId = companyId ?? CompanyId;
        var userRole = _userRoles.SingleOrDefault(ur =>
            ur.RoleId == roleId && ur.CompanyId == targetCompanyId && ur.ToDate is null);
        userRole?.Terminate();
    }

    public void AddToAgent(Agent agent)
    {
        // Terminate any existing agent assignment before adding a new one
        _agentUsers.Where(au => au.ToDate is null).ToList().ForEach(au => au.Terminate());
        _agentUsers.Add(new AgentUser(Id, agent.Id));
    }

    public void AddModulePermission(ModulePermission modulePermission)
    {
        if (_userModulePermissions.Any(ump => ump.ModulePermission == modulePermission))
            return;

        _userModulePermissions.Add(new UserModulePermission(this, modulePermission));
    }

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
        NormalizedUserName = userName.ToUpperInvariant();
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        Contact = contact;
        EntryByUserId = updatedByUserId;
    }
}
