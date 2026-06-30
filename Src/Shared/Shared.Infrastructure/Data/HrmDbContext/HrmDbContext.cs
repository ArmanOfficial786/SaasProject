using System.Reflection;
using Shared.Domain.Abstractions;
using UserManagement.Domain.Entities.BaseEntities;

namespace Shared.Infrastructure.Data.HrmDbContext;

public class HrmDbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
{
    private readonly ITenantContext _tenantContext;
    private IDbContextTransaction? _currentTransaction;

    // Cached MethodInfo for the generic tenant filter
    private static readonly MethodInfo SetTenantQueryFilterMethod =
        typeof(HrmDbContext).GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleModulePermission> RolePermissions => Set<RoleModulePermission>();

    public HrmDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<HrmDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmDbContext).Assembly);

        ApplyAuditRelationships(modelBuilder);
        ApplyTenantQueryFiltersByConvention(modelBuilder);
    }

    // --- Tenant Filter ---

    private void ApplyTenantQueryFiltersByConvention(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.BaseType != null) continue;          // TPH root only
            if (entityType.IsOwned()) continue;                 // owned types follow owner
            if (entityType.FindProperty("CompanyId") is null) continue;

            SetTenantQueryFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<Guid>(e, "CompanyId") == _tenantContext.CompanyId);
    }

    // --- Audit Relationships ---

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

    // --- Automatic CompanyId Stamping (Shadow & Explicit) ---

    public override int SaveChanges()
    {
        StampTenant();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenant();
        return await base.SaveChangesAsync(cancellationToken);
    }

    // Explicit implementation of IDbContext.SaveChangesAsync() (parameterless)
    async Task<int> IDbContext.SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }

    private void StampTenant()
    {
        var companyId = _tenantContext.CompanyId;
        if (companyId == Guid.Empty) return; // unauthenticated – fail closed

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var property = entry.Metadata.FindProperty("CompanyId");
            if (property is null || property.ClrType != typeof(Guid)) continue;

            var current = entry.Property("CompanyId").CurrentValue;
            if (current is Guid guid && guid != Guid.Empty) continue; // already set (explicit)

            entry.Property("CompanyId").CurrentValue = companyId;
        }
    }

    // --- Transaction Support ---

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
            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync();
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
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
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
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

    // --- IDbContext ---

    DbSet<T> IDbContext.Set<T>() => Set<T>();
}
