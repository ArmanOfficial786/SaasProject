using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserManagementInfrastructure(
        this IServiceCollection services)
    {
        // Note: DbContext is already registered in Shared.Infrastructure.DependencyInjection
        // via AddHrmDbContext method. Identity is registered in Program.cs or host configuration.
        return services;
    }
}
