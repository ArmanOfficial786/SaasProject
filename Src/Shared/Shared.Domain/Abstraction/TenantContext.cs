namespace Shared.Domain.Abstraction
{
    public class TenantContext : ITenantContext
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
