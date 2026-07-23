namespace Shared.Application.Interface;

public interface IRepository<T> : IDisposable where T : class
{
    IQueryable<T> SqlQuery(string sql, params object[] parameters);

    Task<int> ExecuteSqlCommandAsync(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters);
    IQueryable<T> All { get; }

    public Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    );

    public Task<TResult?> GetSingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    );

    public Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    );

    public Task<List<TResult>> GetListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool disableTracking = true,
        CancellationToken cancellationToken = default
    );

    public Task<PaginatedData<T>> GetPaginatedListAsync(
        Filter filter,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    public Task<PaginatedData<TResult>> GetPaginatedListAsync<TResult>(
        Filter filter,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    IQueryable<T> GetAll(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object?>>[] includes
    );

    T? GetById(object id);
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    void Insert(T entity);
    Task<T> InsertAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Returns true if any entity satisfies the predicate (or if any entity exists at all).
    /// </summary>
    Task<bool> GetAnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Inserts a range of entities in one batch.
    /// </summary>
    Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    // Task<List<T>> SqlQueryAsync(string query, List<SqlParameter> parameters);
    // Task<string> SqlQueryScalar(string query, List<SqlParameter> parameters);

}
