//using System.Collections;
//using Shared.Domain.Abstractions;

//namespace Shared.Infrastructure.Repositories;

//public class UnitOfWork : IUnitOfWork
//{
//    private readonly IDbContext _context;
//    public bool _disposed;
//    private readonly IPublisher _publisher;
//    private readonly IConfigurationProvider _mapperConfig;
//    private Hashtable _repositories = [];
//    private readonly IServiceProvider _serviceProvider;

//    public UnitOfWork(IDbContext context, IPublisher publisher, IConfigurationProvider mapperConfig, IServiceProvider serviceProvider)
//    {
//        _context = context;
//        _publisher = publisher;
//        _mapperConfig = mapperConfig;
//        _serviceProvider = serviceProvider;
//    }



//    public void BeginTransaction()
//    {
//        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
//        dbContext?.Database.BeginTransaction();
//    }

//    public int Commit()
//    {
//        return SaveChanges();
//    }

//    public async Task<int> CommitAsync()
//    {
//        return await SaveChangesAsync();
//    }
//    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
//    {
//        var type = typeof(TEntity).Name;
//        var repo = _repositories[type];
//        if (_repositories.ContainsKey(type) && repo != null)
//            return (IRepository<TEntity>)repo;
//        else
//        {
//            var repositoryType = typeof(GenericRepository<>);
//            IRepository<TEntity>? newRepo;
//            try
//            {
//                newRepo = _serviceProvider.GetService<IRepository<TEntity>>();
//            }
//            catch (InvalidOperationException)
//            {
//                newRepo = null;
//            }

//            if (newRepo != null)
//                _repositories.Add(type, newRepo);
//            else
//                _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context, _mapperConfig));
//            repo = _repositories[type];
//            if (repo != null)
//                return (IRepository<TEntity>)repo;
//            else
//                throw new Exception("Repository could not be added");
//        }
//    }
//    public void Rollback()
//    {
//        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
//        dbContext?.Database.RollbackTransaction();
//    }

//    public int SaveChanges()
//    {
//        return _context.SaveChanges();
//    }

//    // FIX #3: snapshot events BEFORE saving, dispatch AFTER.
//    // If SaveChangesAsync fails, no event is published.
//    // If publishing fails, the user record is already safe in DB and you can replay the event.
//    public async Task<int> SaveChangesAsync()
//    {
//        return await SaveChangesAsync(CancellationToken.None);
//    }

//    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//    {
//        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
//        if (dbContext == null)
//            return 0;

//        var entitiesWithEvents = dbContext.ChangeTracker.Entries()
//            .Select(e => e.Entity)
//            .OfType<IHasDomainEvents>()
//            .Where(e => e.DomainEvents.Count > 0)
//            .ToList();

//        // Commit first
//        var result = await dbContext.SaveChangesAsync(cancellationToken);

//        // Dispatch after commit
//        foreach (var entity in entitiesWithEvents)
//        {
//            var events = entity.DomainEvents.ToList();
//            entity.ClearDomainEvents();
//            foreach (var domainEvent in events)
//                await _publisher.Publish(domainEvent, cancellationToken);
//        }

//        return result;
//    }

//    public void Dispose()
//    {
//        (_context as IDisposable)?.Dispose();
//    }
//}








// File: Shared.Infrastructure/Repositories/UnitOfWork.cs
using System.Collections;
using Shared.Domain.Abstractions;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;   // <-- alias resolves the namespace/type collision

namespace Shared.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private readonly IPublisher _publisher;
    private readonly IConfigurationProvider _mapperConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly Hashtable _repositories = [];
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

    public void BeginTransaction()
    {
        var dbContext = RequireEfContext();
        dbContext.Database.BeginTransaction();
    }

    public int Commit()
    {
        var result = SaveChanges();
        RequireEfContext().Database.CommitTransaction();
        return result;
    }

    public async Task<int> CommitAsync()
    {
        var result = await SaveChangesAsync();
        var dbContext = RequireEfContext();
        if (dbContext.Database.CurrentTransaction != null)
            await dbContext.Database.CommitTransactionAsync();
        return result;
    }

    public void Rollback()
    {
        RequireEfContext().Database.RollbackTransaction();
    }

    // FIX: alias instead of bare `DbContext` — bare name binds to the
    // Shared.Infrastructure.DbContext namespace in this project, not the EF type.
    private EfDbContext RequireEfContext() =>
        _context as EfDbContext
            ?? throw new InvalidOperationException(
                "UnitOfWork transaction methods require an EF Core DbContext implementation of IDbContext.");

    public int SaveChanges() => _context.SaveChanges();

    public Task<int> SaveChangesAsync() => SaveChangesAsync(CancellationToken.None);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = _context as EfDbContext;   // FIX: same alias here (this was line 226/227)
        if (dbContext == null)
            return await _context.SaveChangesAsync(cancellationToken);

        var entitiesWithEvents = dbContext.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var result = await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            (_context as IDisposable)?.Dispose();

            if (_repositories.Values.OfType<IDisposable>().Any())
            {
                foreach (IDisposable repository in _repositories.Values)
                    repository.Dispose();
            }
        }
        _disposed = true;
    }
}
