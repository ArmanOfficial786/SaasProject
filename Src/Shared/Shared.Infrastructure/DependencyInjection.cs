
using Shared.Application.SeedData;
using Shared.Domain.Abstractions;


namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();

        // Register ITenantContext with a default implementation
        services.AddScoped<ITenantContext>(provider => new TenantContext());
        // Register the domain events interceptor (shared across all DbContexts)
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // Register AutoMapper - scan all assemblies for profiles
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }

    public static IServiceCollection AddHrmDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HrmDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<HrmDbContext>());

        //services.AddDbContext<HrmDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        //services.AddScoped<IDbContext>(provider => provider.GetRequiredService<HrmDbContext>());
        return services;
    }

    public static IServiceCollection AddSchoolDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SchoolDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());

        //services.AddDbContext<SchoolDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        //services.AddScoped<IDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());
        return services;
    }
}
