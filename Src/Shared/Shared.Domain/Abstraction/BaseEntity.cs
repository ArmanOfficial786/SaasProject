namespace Shared.Domain.Abstraction
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public abstract class TenantEntity : BaseEntity
    {
        public Guid TenantId { get; set; }
    }
}
