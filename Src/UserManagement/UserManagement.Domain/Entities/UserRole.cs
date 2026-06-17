//junction table for many-to-many relationship between User and Role
namespace UserManagement.Domain.Entities;

public class UserRole
{

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
