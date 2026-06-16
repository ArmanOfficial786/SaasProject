using Shared.Application.Interface;

namespace Shared.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    public void BeginTransaction();
    public int Commit();
    public Task<int> CommitAsync();
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    public void Rollback();
    public int SaveChanges();
    public Task<int> SaveChangesAsync();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
