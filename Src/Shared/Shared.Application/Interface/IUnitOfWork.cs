namespace Shared.Application.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // ─── Synchronous Transaction Methods ───
    void BeginTransaction();
    int Commit();
    void Rollback();

    // ─── Asynchronous Transaction Methods ───
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync();
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);

    // ─── Save Changes ───
    int SaveChanges();
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ─── Repository Access ───
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
}
