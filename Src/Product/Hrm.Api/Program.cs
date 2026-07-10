using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.Application.SeedData;
using Shared.Infrastructure;
using Shared.Infrastructure.Data.HrmDbContext;
using UserManagement.Application;

public static class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // ─── Serilog (optional, but matches reference) ───
        //builder.Host.UseSerilog((context, config) =>
        //    config.ReadFrom.Configuration(context.Configuration));

        // ─── CORS ───
        var corsAllow = builder.Configuration["AppConfig:CORSAllow"] ?? "";

        _ = builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowHrmWeb", policy =>
            {
                policy.WithOrigins(corsAllow)
                      .WithHeaders("Authorization", "Content-Type")
                      .AllowAnyMethod();
            });
        });
        // ─── Controllers ───
        _ = builder
             .Services.AddControllers()
             .AddJsonOptions(options =>
                 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
             );
        // ─── Swagger / OpenAPI ───
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen();


        // ─── Shared & Application Services ───
        // Shared.Infrastructure (ICurrentUserService, IUnitOfWork, IRepository, Interceptors)
        builder.Services.AddSharedInfrastructure();
        // Shared.Application (MediatR, AutoMapper, etc.)
        builder.Services.AddSharedApplication(builder.Configuration);
        // Shared DbContext (HrmDbContext) – the central database context
        builder.Services.AddHrmDbContext(builder.Configuration.GetConnectionString("HrmConnection")!);
        // Identity — bound specifically to HrmDbContext for this host
        builder.Services.AddIdentityInfrastructure<HrmDbContext>();
        // JWT bearer authentication — context-agnostic
        builder.Services.AddJwtAuthentication(builder.Configuration);

        // UserManagement Application (Commands, Handlers, AutoMapper profiles)
        builder.Services.AddApplication();


        // ─── Token Lifetime for Password Reset ───
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
             options.TokenLifespan = TimeSpan.FromMinutes(double.Parse(builder.Configuration["AppConfig:PasswordResetTokenLifeTime:Minute"]!))
         );


        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HrmDbContext>();
            db.Database.Migrate();

            // Seed runtime-dependent data (Users with password hashing, UserRoles, AgentUser, AgentRole)
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            dbInitializer.SeedAsync().GetAwaiter().GetResult();
        }
        _ = app.UseHttpsRedirection();

        _ = app.UseExceptionHandler(options => { });
        _ = app.UseCors("AllowHrmWeb");
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Run();

        //builder.Services.AddSwaggerGen();

        //// Add HRM DbContext with connection string
        //var hrmConnectionString = builder.Configuration.GetConnectionString("HrmConnection")
        //    ?? throw new InvalidOperationException("Connection string 'HrmConnection' not found.");
        //builder.Services.AddHrmDbContext(hrmConnectionString);


        ////// Add Shared.Infrastructure services (UnitOfWork, Repository pattern)
        ////builder.Services.AddSharedInfrastructure();

        //var app = builder.Build();

        //// Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}


        //app.UseHttpsRedirection();

        //// Seed the database with initial data - handle errors gracefully
        //try
        //{
        //    await app.InitialiseDatabaseAsync();
        //}
        //catch (Exception ex)
        //{
        //    var logger = app.Services.GetRequiredService<ILogger<Program>>();
        //    logger.LogWarning(ex, "Database seeding encountered an error, but the application will continue running.");
        //}


        //app.Run();




    }

}

