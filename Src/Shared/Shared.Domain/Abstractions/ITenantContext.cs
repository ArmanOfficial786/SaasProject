namespace Shared.Domain.Abstractions;

/// <summary>
/// Request-scoped tenant context used by the Infrastructure layer (DbContext, Repositories).
/// This is set by the TenantResolutionMiddleware once per request.
/// </summary>
public interface ITenantContext
{
    Guid CompanyId { get; }
    Guid UserId { get; }
    string ProductCode { get; }
    IReadOnlyList<string> Permissions { get; }
}

/// <summary>
/// Mutable implementation of ITenantContext (used only by middleware).
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Accessor to set/get the TenantContext for the current request.
/// </summary>
public interface ITenantContextAccessor
{
    TenantContext? Context { get; }
    void SetContext(TenantContext context);
}

public class TenantContextAccessor : ITenantContextAccessor
{
    private TenantContext? _context;
    public TenantContext? Context => _context;
    public void SetContext(TenantContext context) => _context = context;
}
