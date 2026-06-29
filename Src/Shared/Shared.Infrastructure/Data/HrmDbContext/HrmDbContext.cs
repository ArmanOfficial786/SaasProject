using System.Reflection;
using Shared.Domain.Abstractions;
using UserManagement.Domain.Entities.BaseEntities;

namespace Shared.Infrastructure.Data.HrmDbContext;

public class HrmDbContext(DbContextOptions<HrmDbContext> options, ITenantContext tenantContext)
    : Microsoft.EntityFrameworkCore.DbContext(options), IDbContext
{
    private readonly ITenantContext _tenantContext = tenantContext;
    private IDbContextTransaction? _currentTransaction;

    // Cached MethodInfo for the generic tenant filter – built once per app lifetime.
    private static readonly MethodInfo SetTenantQueryFilterMethod =
        typeof(HrmDbContext).GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

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

        ApplyAuditRelationships(modelBuilder);
        ApplyTenantQueryFiltersByConvention(modelBuilder);

    }

    /// <summary>
    /// Method 3, convention-based: no marker interface, no BaseEntity change required.
    /// Any entity (regardless of what it inherits — BaseEntity, IdentityUser, plain POCO)
    /// that has a property named "CompanyId" is automatically tenant-scoped.
    /// - Skips TPH-derived types (entityType.BaseType != null) so the filter is set once
    ///   at the root and EF Core propagates it down the whole hierarchy.
    /// - Skips owned types — they inherit their owner's filter automatically.
    /// - Company has no CompanyId property, so it's naturally excluded — no hardcoded
    ///   type-check needed.
    /// </summary>
    private void ApplyTenantQueryFiltersByConvention(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.BaseType != null) continue;                 // only the TPH root
            if (entityType.IsOwned()) continue;                        // owned types follow their owner
            if (entityType.FindProperty("CompanyId") is null) continue; // No CompanyId? No filter.

            // Use reflection to call SetTenantQueryFilter<TEntity> with the runtime type.
            SetTenantQueryFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, [modelBuilder]);
        }
    }

    // EF.Property<Guid>(e, "CompanyId") reads the column by name through EF's metadata,
    // so TEntity needs no interface and no compile-time CompanyId member at all.
    // `this._tenantContext.CompanyId` is referenced through the DbContext instance (not a
    // local variable) so EF re-evaluates it per query instead of baking a snapshot value
    // into the cached model on first build.
    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<Guid>(e, "CompanyId") == this._tenantContext.CompanyId);
    }

    private static void ApplyAuditRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany<AuditableEntity>()
            .WithOne(ae => ae.EntryBy)
            .HasForeignKey(ae => ae.EntryByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany<AuditableEntity>()
            .WithOne(ae => ae.UpdatedBy)
            .HasForeignKey(ae => ae.UpdatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
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
