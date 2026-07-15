//namespace Shared.Application.Interfaces;

//public interface IUnitOfWork : IDisposable
//{
//    public void BeginTransaction();
//    public int Commit();
//    public void Rollback();
//    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
//    Task<int> CommitAsync(CancellationToken cancellationToken = default);
//    Task RollbackAsync(CancellationToken cancellationToken = default);
//    public Task<int> CommitAsync();
//    public IRepository<TEntity> Repository<TEntity>() where TEntity : class;

//    public int SaveChanges();
//    public Task<int> SaveChangesAsync();
//    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
//}




// File: Shared.Application/Interfaces/IUnitOfWork.cs
namespace Shared.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // ✅ Synchronous Transaction Methods
    void BeginTransaction();
    int Commit();
    void Rollback();

    // ✅ Asynchronous Transaction Methods
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync();  // Parameterless version
    Task<int> CommitAsync(CancellationToken cancellationToken);  // With cancellation token
    Task RollbackAsync(CancellationToken cancellationToken = default);

    // ✅ Save Changes
    int SaveChanges();
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // ✅ Repository Access
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
}
