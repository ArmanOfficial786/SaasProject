namespace Shared.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected IDbContext context;
    protected DbSet<T> dbSet;
    protected bool _disposed;
    protected readonly AutoMapper.IConfigurationProvider _mapperConfig;

    public GenericRepository(IDbContext context, AutoMapper.IConfigurationProvider mapperConfig)
    {
        this.context = context;
        dbSet = context.Set<T>();
        _mapperConfig = mapperConfig;
    }

    public IQueryable<T> All => dbSet.AsQueryable();

    public async Task<bool> AllAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
        => await dbSet.AllAsync(predicate, cancellationToken);

    public void Delete(T entity)
    {
        dbSet.Remove(entity);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
            context.Dispose();
        _disposed = true;
    }

    public async Task<int> ExecuteSqlCommandAsync(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
    {
        await Task.Delay(0);
        throw new NotImplementedException();
    }

    public async Task<TResult?> GetSingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<T> query = dbSet;
        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ProjectTo<TResult>(_mapperConfig).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<T> query = dbSet;
        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<T> query = dbSet;
        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ProjectTo<TResult>(_mapperConfig).ToListAsync(cancellationToken);
        }
        else
        {
            return await query.ProjectTo<TResult>(_mapperConfig).ToListAsync(cancellationToken);
        }
    }

    public async Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<T> query = dbSet;
        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync(cancellationToken);
        }
        else
        {
            return await query.ToListAsync(cancellationToken);
        }
    }

    private async Task<PaginatedData<TResult>> BaseGetPaginatedListAsync<TResult>(
        Filter filter,
        Expression<Func<T, bool>>? predicate,
        Func<IQueryable<T>, Task<List<TResult>>> fetcher,
        CancellationToken cancellationToken
    )
    {
        IQueryable<T> query = dbSet;
        query = query.AsNoTracking();

        foreach (var param in filter.Params)
        {
            if (!string.IsNullOrWhiteSpace(param.Value))
            {
                switch (param.Option)
                {
                    case FilterOption.StartsWith:
                        query = query.Where(x => EF.Property<string>(x, param.Key).StartsWith(param.Value));
                        break;
                    case FilterOption.EndsWith:
                        query = query.Where(x => EF.Property<string>(x, param.Key).EndsWith(param.Value));
                        break;
                    case FilterOption.Contains:
                        query = query.Where(x => EF.Property<string>(x, param.Key).Contains(param.Value));
                        break;
                    case FilterOption.DoesNotContain:
                        query = query.Where(x => !EF.Property<string>(x, param.Key).Contains(param.Value));
                        break;
                    case FilterOption.IsEmpty:
                        query = query.Where(x => string.IsNullOrEmpty(x.GetType().GetProperty(param.Key)!.GetValue(x, null)!.ToString()));
                        break;
                    case FilterOption.IsNotEmpty:
                        query = query.Where(x => !string.IsNullOrEmpty(x.GetType().GetProperty(param.Key)!.GetValue(x, null)!.ToString()));
                        break;
                    case FilterOption.IsGreaterThan:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => ConvertToComparable(x, value) > 0));
                        break;
                    case FilterOption.IsGreaterThanOrEqualTo:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => ConvertToComparable(x, value) >= 0));
                        break;
                    case FilterOption.IsLessThan:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => ConvertToComparable(x, value) < 0));
                        break;
                    case FilterOption.IsLessThanOrEqualTo:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => ConvertToComparable(x, value) <= 0));
                        break;
                    case FilterOption.IsEqualTo:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => x == value));
                        break;
                    case FilterOption.IsNotEqualTo:
                        query = query.Where(x => ApplyComparisonFilter(x, param, (x, value) => ConvertToComparable(x, value) != 0));
                        break;
                    default:
                        break;
                }
            }
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var count = (uint)await query.CountAsync(cancellationToken);

        if (filter.PageSize == 0 || count < (filter.PageNumber - 1) * filter.PageSize)
        {
            filter.PageNumber = 1;
            filter.PageSize = count;
        }

        foreach (var sortParam in filter.Sort)
        {
            if (!string.IsNullOrWhiteSpace(sortParam.Field))
            {
                query = sortParam.SortOrder == SortOrder.Asc
                    ? query.OrderBy(x => EF.Property<object>(x, sortParam.Field))
                    : query.OrderByDescending(x => EF.Property<object>(x, sortParam.Field));
            }
        }

        var rows = await fetcher(query
             .Skip((int)((filter.PageNumber - 1) * filter.PageSize))
             .Take((int)filter.PageSize));

        return new PaginatedData<TResult>(rows, count, filter.PageNumber, filter.PageSize);
    }

    public async Task<PaginatedData<T>> GetPaginatedListAsync(
        Filter filter,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return await BaseGetPaginatedListAsync<T>(filter, predicate, (query) => query.ToListAsync(cancellationToken), cancellationToken);
    }

    public async Task<PaginatedData<TResult>> GetPaginatedListAsync<TResult>(
        Filter filter,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return await BaseGetPaginatedListAsync<TResult>(
            filter,
            predicate,
            (query) => query.ProjectTo<TResult>(_mapperConfig).ToListAsync(cancellationToken),
            cancellationToken
        );
    }

    private bool ApplyComparisonFilter<TModel>(TModel? model, FilterParam param, Func<object?, object?, bool> comparison)
    {
        if (model == null) return false;
        var propertyValue = model.GetType().GetProperty(param.Key)!.GetValue(model, null);
        if (propertyValue == null) return false;
        var convertedValue = Convert.ChangeType(param.Value, propertyValue.GetType());
        return comparison(model, convertedValue);
    }

    private int ConvertToComparable(object? obj1, object? obj2)
    {
        if (obj1 is IComparable && obj2 is IComparable)
        {
            return ((IComparable)obj1).CompareTo(obj2);
        }
        else
        {
            throw new ArgumentException("Objects must implement IComparable interface.");
        }
    }

    public IQueryable<T> GetAll(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object?>>[] includes
    )
    {
        IQueryable<T> query = dbSet;

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        if (includes != null)
            foreach (var include in includes)
                query = query.Include(include);

        return query;
    }

    public T? GetById(object id) => dbSet.Find(id);

    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        => await dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

    public void Insert(T entity) => dbSet.Add(entity);

    public async Task<T> InsertAsync(T entity)
    {
        _ = await dbSet.AddAsync(entity);
        return entity;
    }

    public IQueryable<T> SqlQuery(string sql, params object[] parameters) => dbSet.FromSqlRaw(sql, parameters);

    public void Update(T entity) => dbSet.Update(entity);

    public async Task<bool> GetAnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = dbSet.AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await dbSet.AddRangeAsync(entities, cancellationToken);
    }

}
