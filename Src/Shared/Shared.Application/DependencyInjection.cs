using FluentValidation;
using MediatR;
using Shared.Application.Behaviors;
using Shared.Application.Configuration;
using Shared.Application.SeedData;

namespace Shared.Application;

/// <summary>
/// Dependency injection extensions for Shared.Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Shared.Application services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSharedApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AutoMapper from this assembly - use the Assembly overload
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

        // Register AppConfig options using BindConfiguration
        _ = services.Configure<AppConfig>(
            options => configuration.GetSection("AppConfig").Bind(options));

        // Register MailConfig options using BindConfiguration
        _ = services.Configure<MailConfig>(
            options => configuration.GetSection("SMTPConfig").Bind(options));

        // Register DbInitializer for seeding data
        _ = services.AddScoped<DbInitializer>();

        // Register FluentValidation validators from this assembly
        _ = services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        return services;
    }
}
