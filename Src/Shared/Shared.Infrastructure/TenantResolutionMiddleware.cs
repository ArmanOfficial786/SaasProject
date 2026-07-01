using Microsoft.AspNetCore.Http;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware that resolves the tenant (Company) from the JWT claims,
/// validates it, and sets the ITenantContext for the current request.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, HrmDbContext dbContext, ITenantContextAccessor accessor)
    {
        // Read claims (same as CurrentUserService does)
        var companyIdStr = context.User.FindFirst("CompanyId");
        var userIdStr = context.User.FindFirst("UserId");
        var permissions = context.User.FindAll("permission").Select(c => c.Value).ToList();

        if (Guid.TryParse(companyIdStr?.Value, out var companyId) && Guid.TryParse(userIdStr?.Value, out var userId))
        {
            // Validate that the tenant CompanyId is valid (no DB call needed - it's just a GUID identifier)
            // The CompanyId is used for multi-tenant isolation and should be a valid GUID
            if (companyId == Guid.Empty)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid company ID.");
                return;
            }

            // Set the context for the rest of the request
            accessor.SetContext(new TenantContext
            {
                CompanyId = companyId,
                UserId = userId,
                ProductCode = string.Empty,
                Permissions = permissions
            });
        }

        await _next(context);
    }
}
