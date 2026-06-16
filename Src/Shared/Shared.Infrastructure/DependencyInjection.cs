////namespace Shared.Infrastructure;

/////// <summary>
/////// Dependency injection extensions for Shared.Infrastructure layer
/////// </summary>
////public static class DependencyInjection
////{
////    /// <summary>
////    /// Adds Shared.Infrastructure services to the dependency injection container
////    /// </summary>
////    /// <param name="services">Service collection</param>
////    /// <returns>Service collection for chaining</returns>
////    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
////    {
////        // Register Unit of Work pattern
////        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

////        return services;
////    }

////    /// <summary>
////    /// Adds HRM DbContext and related services
////    /// </summary>
////    /// <param name="services">Service collection</param>
////    /// <param name="connectionString">HRM database connection string</param>
////    /// <returns>Service collection for chaining</returns>
////    public static IServiceCollection AddHrmDbContext(this IServiceCollection services, string connectionString)
////    {
////        // Register HrmDbContext
////        _ = services.AddDbContext<HrmDbContext>(options =>
////            options.UseSqlServer(connectionString));

////        // Register IDbContext with HrmDbContext
////        _ = services.AddScoped<IDbContext>(provider => provider.GetRequiredService<HrmDbContext>());

////        return services;
////    }

////    /// <summary>
////    /// Adds School DbContext and related services
////    /// </summary>
////    /// <param name="services">Service collection</param>
////    /// <param name="connectionString">School database connection string</param>
////    /// <returns>Service collection for chaining</returns>
////    public static IServiceCollection AddSchoolDbContext(this IServiceCollection services, string connectionString)
////    {
////        // Register SchoolDbContext
////        _ = services.AddDbContext<SchoolDbContext>(options =>
////            options.UseSqlServer(connectionString));

////        // Register IDbContext with SchoolDbContext
////        _ = services.AddScoped<IDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());

////        return services;
////    }
////}








//// ✅ ADD the missing using for HrmDbContext (adjust namespace to match your project)
//using Hrm.Persistence.Data;  // or using Shared.Infrastructure.DbContext.HrmDbContext;
//// If using Shared.Infrastructure.DbContext.HrmDbContext, then:
//// using Shared.Infrastructure.DbContext.HrmDbContext;

//namespace Shared.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
//    {
//        services.AddScoped<IUnitOfWork, UnitOfWork>();
//        return services;
//    }

//    public static IServiceCollection AddHrmDbContext(this IServiceCollection services, string connectionString)
//    {
//        services.AddDbContext<HrmDbContext>(options =>
//            options.UseSqlServer(connectionString));

//        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<HrmDbContext>());
//        return services;
//    }

//    public static IServiceCollection AddSchoolDbContext(this IServiceCollection services, string connectionString)
//    {
//        services.AddDbContext<SchoolDbContext>(options =>
//            options.UseSqlServer(connectionString));

//        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());
//        return services;
//    }
//}



using Shared.Infrastructure.Data.HrmDbContext;
using Shared.Application.SeedData;
using Microsoft.Extensions.Logging;
using Shared.Domain.Abstraction;
using AutoMapper;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();

        // Register ITenantContext with a default implementation
        services.AddScoped<ITenantContext>(provider => new TenantContext());

        // Register AutoMapper - scan all assemblies for profiles
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }

    public static IServiceCollection AddHrmDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HrmDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<HrmDbContext>());
        return services;
    }

    public static IServiceCollection AddSchoolDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SchoolDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<SchoolDbContext>());
        return services;
    }
}