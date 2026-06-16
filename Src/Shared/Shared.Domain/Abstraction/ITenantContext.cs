namespace Shared.Domain.Abstraction
{
    public interface ITenantContext
    {
        Guid TenantId { get; }
        Guid UserId { get; }
        string ProductCode { get; }
        IReadOnlyList<string> Permissions { get; }

    }
}
