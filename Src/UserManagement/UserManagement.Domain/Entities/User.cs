

namespace UserManagement.Domain.Entities;

public class User : IdentityUser<Guid>

{
    [MaxLength(256)]
    public new string? Email { get; private set; }
    [MaxLength(100)]
    public string? FullName { get; private set; }
    [MaxLength(256)]
    public string? Contact { get; private set; }
    [MaxLength(256)]
    public new string? PasswordHash { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public User? EntryBy { get; private set; }
    public DateTime EntryDate { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public Guid CompanyId { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<Permission> _userPermissions = [];
    public IReadOnlyCollection<Permission> UserPermissions => _userPermissions.AsReadOnly();

    private readonly List<UserStatus> _userStatuses = [];
    public IReadOnlyCollection<UserStatus> UserStatuses => _userStatuses.AsReadOnly();

    public User(string? email, string? fullName, string? contact, string? passwordHash, User? entryBy, DateTime entryDate)
    {
        Email = email;
        FullName = fullName;
        Contact = contact;
        PasswordHash = passwordHash;
        IsEmailConfirmed = false;
        FailedLoginAttempts = 0;
        EntryBy = entryBy;
        EntryDate = entryDate;
        AddStatus(new UserStatus());
    }

    private User() { } // For EF Core

    private void AddStatus(UserStatus status)
    {
        _userStatuses.Add(status);
    }

    public void AddRole(Role role)
    {
        _userRoles.Add(new UserRole(role));
    }

}
