using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Service;


public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid? UserId
    {
        get
        {
            var userId = GetClaimValue("UserId");
            return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
        }
    }
    public string? UserName
    {
        get
        {
            var userName = GetClaimValue("UserName");
            return string.IsNullOrEmpty(userName) ? null : userName;
        }
    }
    public int? CompanyId
    {
        get
        {
            var companyId = GetClaimValue("CompanyId");
            try
            {
                return string.IsNullOrEmpty(companyId) ? null : int.Parse(companyId);
            }
            catch
            {
                return null;
            }
        }
    }
    public Guid? AgentId => null;
    public Guid? BranchId => null;
    public Guid? CustomerId
    {
        get
        {
            var customerId = GetClaimValue("CustomerId");
            try
            {
                return string.IsNullOrEmpty(customerId) ? null : Guid.Parse(customerId);
            }
            catch
            {
                return null;
            }
        }
    }

    public UserInfo? UserInfo
    {
        get
        {
            if (UserId.HasValue && !string.IsNullOrEmpty(UserName))
                return new(UserId ?? Guid.Empty, UserName, GetClaimValue("Name")!);
            else
                return null;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        try
        {
            return (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
                ? _httpContextAccessor.HttpContext?.User?.Claims.Single(c => c.Type == claimType).Value ?? null
                : null;
        }
        catch
        {
            return null;
        }
    }
}
