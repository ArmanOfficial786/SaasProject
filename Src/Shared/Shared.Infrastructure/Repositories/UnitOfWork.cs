using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Application.Interface;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private readonly IPublisher _publisher;
    private readonly IConfigurationProvider _mapperConfig;
    private readonly Dictionary<Type, object> _repositories = [];

    public UnitOfWork(IDbContext context, IPublisher publisher, IConfigurationProvider mapperConfig)
    {
        _context = context;
        _publisher = publisher;
        _mapperConfig = mapperConfig;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
            _repositories[type] = new GenericRepository<T>(_context, _mapperConfig);
        return (IRepository<T>)_repositories[type];
    }

    public void BeginTransaction()
    {
        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
        dbContext?.Database.BeginTransaction();
    }

    public int Commit()
    {
        return SaveChanges();
    }

    public async Task<int> CommitAsync()
    {
        return await SaveChangesAsync();
    }

    public void Rollback()
    {
        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
        dbContext?.Database.RollbackTransaction();
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    // FIX #3: snapshot events BEFORE saving, dispatch AFTER.
    // If SaveChangesAsync fails, no event is published.
    // If publishing fails, the user record is already safe in DB and you can replay the event.
    public async Task<int> SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = _context as Microsoft.EntityFrameworkCore.DbContext;
        if (dbContext == null)
            return 0;

        var entitiesWithEvents = dbContext.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        // Commit first
        var result = await dbContext.SaveChangesAsync(cancellationToken);

        // Dispatch after commit
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
        (_context as IDisposable)?.Dispose();
    }
}
