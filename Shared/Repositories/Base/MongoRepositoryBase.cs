using MongoDB.Driver;
using Shared.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Shared.Repositories.Base;

/// <summary>
/// Kullanım örneği:
/// public class CartRepository : MongoRepositoryBase&lt;CartDocument&gt;
/// {
///     public CartRepository(IMongoDatabase db) : base(db, "carts") { }
///     protected override string GetId(CartDocument e) => e.Id;
/// }
/// </summary>
public abstract class MongoRepositoryBase<T> : IMongoRepositoryBase<T, string> where T : class
{
    protected readonly IMongoCollection<T> Collection;

    protected MongoRepositoryBase(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await Collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await Collection.Find(_ => true).ToListAsync(ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default)
        => await Collection.Find(filter).ToListAsync(ct);

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default)
        => await Collection.Find(filter).FirstOrDefaultAsync(ct);

    public async Task<long> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default)
    {
        var f = filter is null ? Builders<T>.Filter.Empty : Builders<T>.Filter.Where(filter);
        return await Collection.CountDocumentsAsync(f, cancellationToken: ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }

    public async Task AddManyAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await Collection.InsertManyAsync(entities, cancellationToken: ct);

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", GetId(entity));
        await Collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        await Collection.DeleteOneAsync(filter, ct);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await Collection.Find(filter).AnyAsync(ct);
    }

    protected abstract string GetId(T entity);
}
