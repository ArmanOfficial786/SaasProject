using Shared.Domain.Abstraction;

namespace Shared.Infrastructure.Data.HrmDbContext;

public class HrmDbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
{
    private readonly ITenantContext _tenantContext;
    private IDbContextTransaction? _currentTransaction;

    public HrmDbContext(DbContextOptions<HrmDbContext> options, ITenantContext tenantContext)
        : base(options) => _tenantContext = tenantContext;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmDbContext).Assembly);
        var tenantId = _tenantContext.TenantId;
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(e => e.TenantId == tenantId);
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
