using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Data.HrmDbContext;

public class HrmDbContext(DbContextOptions<HrmDbContext> options, ITenantContext tenantContext)
    : Microsoft.EntityFrameworkCore.DbContext(options), IDbContext
{
    private readonly ITenantContext _tenantContext = tenantContext;
    private IDbContextTransaction? _currentTransaction;

    public DbSet<Company> Tenants => Set<Company>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleModulePermission> RolePermissions => Set<RoleModulePermission>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmDbContext).Assembly);
        // Get the tenant ID from the context (set by middleware)
        var companyId = _tenantContext?.CompanyId ?? Guid.Empty;

        // --- Global query filters for multi-tenancy ---
        // These automatically apply to every query!
        modelBuilder.Entity<User>().HasQueryFilter(u => u.CompanyId == companyId);
        modelBuilder.Entity<Role>().HasQueryFilter(r => r.CompanyId == companyId);
        modelBuilder.Entity<Permission>().HasQueryFilter(p => p.CompanyId == companyId);
        modelBuilder.Entity<Agent>().HasQueryFilter(a => a.CompanyId == companyId);
        modelBuilder.Entity<CompanyRole>().HasQueryFilter(cr => cr.CompanyId == companyId);
    }

    DbSet<T> IDbContext.Set<T>() => Set<T>();
    public async Task<int> SaveChangesAsync() => await SaveChangesAsync(CancellationToken.None);
    public override async Task<int> SaveChangesAsync(CancellationToken ct) => await base.SaveChangesAsync(ct);
    public override int SaveChanges() => base.SaveChanges();

    public void BeginTransaction()
    {
        if (_currentTransaction != null) return;
        _currentTransaction = Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
    }

    public async Task<int> CommitAsync()
    {
        try
        {
            var result = await SaveChangesAsync();
            await (_currentTransaction?.CommitAsync() ?? Task.CompletedTask);
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally { _currentTransaction?.Dispose(); _currentTransaction = null; }
    }

    public int Commit()
    {
        try
        {
            var result = SaveChanges();
            _currentTransaction?.Commit();
            return result;
        }
        catch
        {
            Rollback();
            throw;
        }
        finally { _currentTransaction?.Dispose(); _currentTransaction = null; }
    }

    public void Rollback()
    {
        try { _currentTransaction?.Rollback(); }
        finally { _currentTransaction?.Dispose(); _currentTransaction = null; }
    }

    private async Task RollbackAsync()
    {
        try { if (_currentTransaction != null) await _currentTransaction.RollbackAsync(); }
        finally { _currentTransaction?.Dispose(); _currentTransaction = null; }
    }
}
