using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Data.HrmDbContext;

public class HrmDbContext : IdentityDbContext<User, Role, Guid>, IDbContext
{
    //private readonly ITenantContext _tenantContext;
    private IDbContextTransaction? _currentTransaction;

    public HrmDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<HrmDbContext> options)
       : base(options)
    {

    }

    #region User Management
    public new DbSet<User> Users => Set<User>();
    public new DbSet<Role> Roles => Set<Role>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Security.Domain.Entities.Application> Applications => Set<Security.Domain.Entities.Application>();
    public DbSet<ModulePermission> ModulePermissions => Set<ModulePermission>();
    public DbSet<UserManagement.Domain.Entities.Module> Modules => Set<UserManagement.Domain.Entities.Module>();
    public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();
    public DbSet<UserModulePermission> UserModulePermissions => Set<UserModulePermission>();
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    #endregion



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Domain events — transient, never persisted
        modelBuilder.Ignore<BaseEvent>();

        // Reflection metadata accidentally reachable from an entity graph —
        // never intended to be persisted; see comment below re: root cause
        modelBuilder.Ignore<System.Reflection.CustomAttributeData>();
        modelBuilder.Ignore<System.Reflection.MemberInfo>();
        modelBuilder.Ignore<System.Reflection.Module>();
        modelBuilder.Ignore<Type>();

        // Picks up all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);



    }

    // ── IDbContext ────────────────────────────────────────────────────────────────────────────────────────────

    DbSet<T> IDbContext.Set<T>() => Set<T>();


    // Explicit implementation of IDbContext.SaveChangesAsync() (parameterless)
    async Task<int> IDbContext.SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }

    // Public SaveChanges overload for IDbContext
    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    // Public SaveChangesAsync overload for IDbContext
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public void BeginTransaction()
    {
        if (_currentTransaction != null) return;
        _currentTransaction = this.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
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

    public new void Dispose()
    {
        base.Dispose();
    }
}
