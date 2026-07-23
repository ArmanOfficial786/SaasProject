using Microsoft.EntityFrameworkCore.Design;

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
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=saas_hrm_db3;User Id=sa;password=cosys123;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);



        // ITenantContext gone — constructor takes only DbContextOptions now.
        return new HrmDbContext(optionsBuilder.Options);
    }
}
