using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.SeedData;
using Shared.Infrastructure.GlobalException;
using Shared.Infrastructure.Service;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    // Context-agnostic — safe to call once per host, regardless of which DbContext it uses.
    // This is the ONLY place AddAutoMapper is called across the whole solution —
    // it scans every loaded assembly, so module-specific profiles (UserManagement,
    // Hrm, School, ...) are all picked up from here without needing their own call.
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        // Global exception handling — registered directly, no separate extension method.
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        //to do : add email service
        //services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IHashService, HashService>();
        services.AddTransient<IFileService, LocalStorageFileService>();

        return services;
    }

    public static IServiceCollection AddHrmDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HrmDbContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>()));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<HrmDbContext>());
        return services;
    }

    public static IServiceCollection AddSchoolDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SchoolDbContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>()));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<SchoolDbContext>());
        return services;
    }

    // Generic ONLY here — this is the one place the type parameter earns its keep
    public static IServiceCollection AddIdentityInfrastructure<TContext>(this IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddIdentity<User, Role>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // User settings
            options.User.RequireUniqueEmail = true;
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<TContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["AppConfig:ApiURL"],
            ValidAudience = configuration["AppConfig:WebURL"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["AppConfig:ApiKey"]!))
        };

        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = tokenValidationParameters);

        return services;
    }
}
