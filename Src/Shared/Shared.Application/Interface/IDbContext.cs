using Microsoft.EntityFrameworkCore;

namespace Shared.Application.Interface
{
    //IDbContext is used Exceptionally when we need simple rawsql query, otherwise we should use the IUnitOfWork pattern to manage transactions across multiple repositories.
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        void BeginTransaction();
        int Commit();
        Task<int> CommitAsync();
        void Rollback();
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
