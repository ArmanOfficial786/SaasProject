using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System.Text.Json;
using UserManagement.Domain.Entities;

namespace Shared.Application.SeedData;

public class DbInitializer
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public DbInitializer(IUnitOfWork unitOfWork, ILogger<DbInitializer> logger, IHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded (e.g., any permissions exist)
        var anyPermission = await _unitOfWork.Repository<Permission>().GetAnyAsync();
        if (anyPermission)
        {
            _logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding database...");

        try
        {
            await SeedPermissions();
            var tenant = await SeedTenant();
            var adminRole = await SeedAdminRole(tenant.Id);
            var adminUser = await SeedAdminUser(tenant.Id);
            await AssignRoleToUser(adminUser.UserId, adminRole.RoleId);
            await AssignAllPermissionsToRole(adminRole.RoleId);

            _logger.LogInformation("Seeding completed.");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning("Seed data files not found. Skipping database seeding. Error: {Error}", ex.Message);
        }
    }

    private async Task SeedPermissions()
    {
        // Construct the path relative to the content root
        var jsonPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "SeedData", "AuthData", "permissions.json"
        );

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Could not find permissions.json file at {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var permissions = JsonSerializer.Deserialize<List<Permission>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (permissions == null || !permissions.Any())
            throw new Exception("No permissions found in JSON file.");

        await _unitOfWork.Repository<Permission>().InsertRangeAsync(permissions);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} permissions.", permissions.Count);
    }

    private async Task<Tenant> SeedTenant()
    {
        // Construct the path relative to the content root
        var jsonPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "SeedData", "AuthData", "tenant.json"
        );

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Could not find tenant.json file at {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var tenantData = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (tenantData == null)
            throw new Exception("Failed to deserialize tenant data from JSON.");

        var tenant = new Tenant
        {
            Name = tenantData["name"]?.ToString() ?? throw new Exception("Missing 'name' in tenant data"),
            Subdomain = tenantData["subdomain"]?.ToString() ?? throw new Exception("Missing 'subdomain' in tenant data"),
            ProductCode = tenantData["productCode"]?.ToString() ?? throw new Exception("Missing 'productCode' in tenant data"),
            Status = tenantData["status"]?.ToString() ?? throw new Exception("Missing 'status' in tenant data"),
            SubscriptionStartDate = DateTime.Parse(tenantData["subscriptionStartDate"]?.ToString() ?? throw new Exception("Missing 'subscriptionStartDate' in tenant data")),
            SubscriptionEndDate = DateTime.Parse(tenantData["subscriptionEndDate"]?.ToString() ?? throw new Exception("Missing 'subscriptionEndDate' in tenant data"))
        };

        await _unitOfWork.Repository<Tenant>().InsertAsync(tenant);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Seeded tenant: {Name} with Id {Id}", tenant.Name, tenant.Id);
        return tenant;
    }

    private async Task<Role> SeedAdminRole(int tenantId)
    {
        // Construct the path relative to the content root
        var jsonPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "SeedData", "AuthData", "admin.json"
        );

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Could not find admin.json file at {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var adminData = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (adminData == null)
            throw new Exception("Failed to deserialize admin data from JSON.");

        var roleJson = JsonSerializer.Serialize(adminData["adminRole"] ?? throw new Exception("Missing 'adminRole' in admin data"));
        var role = JsonSerializer.Deserialize<Role>(roleJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? throw new Exception("Failed to deserialize role from JSON.");

        role.RoleId = Guid.NewGuid();
        role.TenantId = Guid.NewGuid();
        role.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Role>().InsertAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }

    private async Task<User> SeedAdminUser(int tenantId)
    {
        // Construct the path relative to the content root
        var jsonPath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "SeedData", "AuthData", "admin.json"
        );

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Could not find admin.json file at {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var adminData = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (adminData == null)
            throw new Exception("Failed to deserialize admin data from JSON.");

        var userJson = JsonSerializer.Serialize(adminData["adminUser"] ?? throw new Exception("Missing 'adminUser' in admin data"));
        var user = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? throw new Exception("Failed to deserialize user from JSON.");

        user.UserId = Guid.NewGuid();
        user.TenantId = Guid.NewGuid();
        // user.Password is already set from JSON
        user.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<User>().InsertAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    private async Task AssignRoleToUser(Guid userId, Guid roleId)
    {
        var userRole = new UserRole { UserId = userId, RoleId = roleId };
        await _unitOfWork.Repository<UserRole>().InsertAsync(userRole);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task AssignAllPermissionsToRole(Guid roleId)
    {
        var permissions = await _unitOfWork.Repository<Permission>().GetListAsync(p => true);
        foreach (var perm in permissions)
        {
            var rolePerm = new RolePermission { RoleId = roleId, PermissionId = perm.PermissionId };
            await _unitOfWork.Repository<RolePermission>().InsertAsync(rolePerm);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
