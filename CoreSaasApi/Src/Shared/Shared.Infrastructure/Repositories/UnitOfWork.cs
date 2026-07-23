using System.Collections;
using Shared.Domain.Abstractions;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace Shared.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private readonly IPublisher _publisher;
    private readonly IConfigurationProvider _mapperConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly Hashtable _repositories = [];
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        IDbContext context,
        IPublisher publisher,
        IMapper mapper,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _publisher = publisher;
        _mapperConfig = mapper.ConfigurationProvider;
        _serviceProvider = serviceProvider;
    }

    // ============================================================
    // REPOSITORY ACCESS
    // ============================================================

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;
        var repo = _repositories[type];
        if (_repositories.ContainsKey(type) && repo != null)
            return (IRepository<TEntity>)repo;

        var repositoryType = typeof(GenericRepository<>);
        IRepository<TEntity>? newRepo;
        try
        {
            newRepo = _serviceProvider.GetService<IRepository<TEntity>>();
        }
        catch (InvalidOperationException)
        {
            newRepo = null;
        }

        if (newRepo != null)
            _repositories.Add(type, newRepo);
        else
            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context, _mapperConfig));

        repo = _repositories[type];
        return repo != null
            ? (IRepository<TEntity>)repo
            : throw new Exception($"Repository for {type} could not be added");
    }

    // ============================================================
    // TRANSACTION MANAGEMENT - Synchronous
    // ============================================================

    public void BeginTransaction()
    {
        var dbContext = RequireEfContext();
        _currentTransaction = dbContext.Database.BeginTransaction();
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
            _currentTransaction?.Rollback();
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
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    // ============================================================
    // TRANSACTION MANAGEMENT - Asynchronous
    // ============================================================

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        var dbContext = RequireEfContext();
        _currentTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    // FIX: forward to the real implementation instead of calling itself
    public Task<int> CommitAsync()
    {
        return CommitAsync(CancellationToken.None);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    // ============================================================
    // SAVE CHANGES
    // ============================================================

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return SaveChangesAsync(CancellationToken.None);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = _context as EfDbContext;
        if (dbContext == null)
            return await _context.SaveChangesAsync(cancellationToken);

        // Snapshot domain events BEFORE saving
        var entitiesWithEvents = dbContext.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        // Save changes to database
        var result = await dbContext.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER successful save
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private EfDbContext RequireEfContext() =>
        _context as EfDbContext
            ?? throw new InvalidOperationException(
                "UnitOfWork transaction methods require an EF Core DbContext implementation of IDbContext.");

    // ============================================================
    // DISPOSAL
    // ============================================================

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            (_context as IDisposable)?.Dispose();

            if (_repositories.Values.OfType<IDisposable>().Any())
            {
                foreach (IDisposable repository in _repositories.Values)
                    repository.Dispose();
            }

            _repositories.Clear();
        }
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_currentTransaction != null)
                await _currentTransaction.DisposeAsync();

            if (_context is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                (_context as IDisposable)?.Dispose();

            foreach (var repository in _repositories.Values.OfType<IAsyncDisposable>())
            {
                await repository.DisposeAsync();
            }

            foreach (var repository in _repositories.Values.OfType<IDisposable>())
            {
                repository.Dispose();
            }

            _repositories.Clear();
        }
        _disposed = true;
    }
}
