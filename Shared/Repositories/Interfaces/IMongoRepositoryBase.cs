namespace Shared.Repositories.Interfaces;

public interface IMongoRepositoryBase<T, TKey>
{
    Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<T>> FindAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> filter,
        CancellationToken ct = default);

    Task<T?> FindOneAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> filter,
        CancellationToken ct = default);

    Task<long> CountAsync(
        System.Linq.Expressions.Expression<Func<T, bool>>? filter = null,
        CancellationToken ct = default);

    Task<T> AddAsync(T entity, CancellationToken ct = default);

    Task AddManyAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task UpdateAsync(T entity, CancellationToken ct = default);

    Task DeleteAsync(TKey id, CancellationToken ct = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
}