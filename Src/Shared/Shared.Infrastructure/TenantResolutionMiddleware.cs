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
            // Validate that the company exists and is active (DB call – only once per request!)
            var company = await dbContext.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Company does not exist.");
                return;
            }

            // Optionally check company status if you have a status field
            // if (company.Status != "Active") { ... }

            // Set the context for the rest of the request
            accessor.SetContext(new TenantContext
            {
                CompanyId = company.Id,
                UserId = userId,
                ProductCode = company.ProductCode ?? string.Empty,
                Permissions = permissions
            });
        }

        await _next(context);
    }
}
