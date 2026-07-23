using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Service;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal
        => _httpContextAccessor.HttpContext?.User;

    // FIX #1 (primary): IsAuthenticated is now a direct, readable property.
    public bool IsAuthenticated
        => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
        => Guid.TryParse(GetClaimValue("UserId"), out var id) ? id : null;

    public string? UserName
        => GetClaimValue("UserName");

    public int? CompanyId
        => int.TryParse(GetClaimValue("CompanyId"), out var id) ? id : null;

    // FIX #5: Actually reads from claims instead of returning null.
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
    // Single() throws when claim is missing or duplicated, both silently swallowed.
    // FirstOrDefault returns null when not found, first when duplicated — no exception.
    private string? GetClaimValue(string claimType)
    {
        if (!IsAuthenticated) return null;
        return Principal?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
