using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Interface;

namespace Shared.Infrastructure.DbContext.SchoolDbContext;

public class SchoolDbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
{
    private IDbContextTransaction? _transaction;

    public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add School-specific entity configurations here
    }

    public void BeginTransaction()
    {
        _transaction = Database.BeginTransaction();
    }

    public int Commit()
    {
        try
        {
            var result = SaveChanges();
            _transaction?.Commit();
            return result;
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public Task<int> CommitAsync()
    {
        throw new NotImplementedException();
    }

    public void Rollback()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync(CancellationToken.None);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
