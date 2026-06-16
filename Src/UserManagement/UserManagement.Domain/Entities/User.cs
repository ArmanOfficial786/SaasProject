namespace UserManagement.Domain.Entities
{
    public class User : TenantEntity
    {
        [Key]
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Status { get; set; } = "InActive";
        public bool IsEmailVerified { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
