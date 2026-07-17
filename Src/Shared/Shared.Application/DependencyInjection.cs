using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Application.Behaviors;
using Shared.Application.Configuration;
using Shared.Application.Identity;
using Shared.Application.SeedData;
using UserManagement.Domain.Entities;

namespace Shared.Application;

/// <summary>
/// Dependency injection extensions for Shared.Application layer.
/// This is referenced by every module's Application layer (UserManagement, Hrm,
/// School, etc.), so anything registered here — MediatR, validators, AutoMapper —
/// is scanned across the whole AppDomain and picked up automatically project-wide.
/// Do NOT re-register MediatR/AutoMapper/validators in individual module projects;
/// that creates duplicate handler/validator registrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AppConfig options — bind, don't just reference the section
        services.Configure<AppConfig>(
            options => configuration.GetSection("AppConfig").Bind(options));

        // MailConfig is consumed as a plain injected singleton (not IOptions<MailConfig>)
        // in handlers like CreateUserCommandHandler, so bind it once here as a singleton.
        // Do NOT also register services.Configure<MailConfig>(...) — that binds a second,
        // independent IOptions<MailConfig> instance that nothing in the app actually uses,
        // and having both is a footgun if one is ever updated without the other.
        services.AddSingleton(sp =>
        {
            var mailConfig = new MailConfig();
            configuration.GetSection("SMTPConfig").Bind(mailConfig);
            return mailConfig;
        });

        // Register DbInitializer for seeding data
        services.AddScoped<DbInitializer>();

        // Register FluentValidation validators from every loaded assembly
        // (covers UserManagement.Application, Hrm.Application, School.Application, etc.)
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register MediatR handlers from every loaded assembly.
        // NOTE: AutoMapper registration intentionally lives ONLY in
        // Shared.Infrastructure.AddSharedInfrastructure() (it does an AppDomain-wide
        // scan there). Registering AddAutoMapper a second time here would add a second
        // IMapper singleton descriptor, and whichever registration runs LAST in
        // Program.cs silently wins — meaning the narrower scan (this assembly only)
        // could shadow the broader one and quietly drop every module's mapping profiles.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        // Replace the default IRoleValidator<Role> with our custom CompanyScopedRoleValidator
        services.RemoveAll<IRoleValidator<Role>>();
        services.AddScoped<IRoleValidator<Role>, CompanyScopedRoleValidator>();

        return services;
    }
}
