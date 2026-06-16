using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interface;
using Shared.Application.Interfaces;
using System.Collections;

namespace Shared.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private bool _disposed;
    private readonly Hashtable _repositories = [];
    private readonly AutoMapper.IConfigurationProvider _mapperConfig;
    private readonly IServiceProvider _serviceProvider;
    public UnitOfWork(IDbContext context, IMapper mapper, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapperConfig = mapper.ConfigurationProvider;
        _serviceProvider = serviceProvider;
    }

    public void BeginTransaction()
    {
        _context.BeginTransaction();
    }

    public int Commit()
    {
        return _context.Commit();
    }

    public Task<int> CommitAsync()
    {
        return _context.CommitAsync();
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
            _context.Dispose();

            if (_repositories != null && _repositories.Values != null && _repositories.Values.OfType<IDisposable>().Any())
            {
                foreach (IDisposable repository in _repositories.Values)
                {
                    repository.Dispose();
                }
            }
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;
        var repo = _repositories[type];
        if (_repositories.ContainsKey(type) && repo != null)
            return (IRepository<TEntity>)repo;
        else
        {
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
            if (repo != null)
                return (IRepository<TEntity>)repo;
            else
                throw new Exception("Repository could not be added");
        }
    }

    public void Rollback()
    {
        _context.Rollback();
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
