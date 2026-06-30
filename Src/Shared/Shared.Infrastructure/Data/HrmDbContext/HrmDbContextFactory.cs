using Microsoft.EntityFrameworkCore.Design;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Data.HrmDbContext;

/// <summary>
/// Factory for creating HrmDbContext instances at design-time
/// This is used by EF Core tooling for migrations
/// </summary>
public class HrmDbContextFactory : IDesignTimeDbContextFactory<HrmDbContext>
{
    public HrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrmDbContext>();

        // Use a default connection string for design-time operations
        // This connection string should match your development database
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=saas_hrm_db1;User Id=sa;password=cosys123;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        // Create a mock ITenantContext for design-time
        // This allows migrations to be created without a full dependency injection container
        var tenantContext = new DesignTimeTenantContext();

        return new HrmDbContext(
            optionsBuilder.Options,
            tenantContext
        );
    }
}

/// <summary>
/// Mock implementation of ITenantContext for design-time database operations
/// </summary>
internal class DesignTimeTenantContext : ITenantContext
{
    public Guid CompanyId => Guid.Empty;
    public Guid UserId => Guid.Empty;
    public string ProductCode => "design-time";
    public IReadOnlyList<string> Permissions => new List<string>();
}
