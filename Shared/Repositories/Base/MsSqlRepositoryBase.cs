using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Shared.Repositories.Base;

/// <summary>
/// Dapper tabanlı MS SQL base repository.
/// Order ve Payment servisleri için.
/// Kullanım:
/// public class OrderRepository : MsSqlRepositoryBase
/// {
///     public OrderRepository(IConfiguration config) : base(config, "OrderPaymentDb") { }
/// }
/// </summary>
public abstract class MsSqlRepositoryBase
{
    private readonly string _connectionString;

    protected MsSqlRepositoryBase(IConfiguration configuration, string connectionStringName)
    {
        _connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
    }

    protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, param);
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, param);
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<T>(sql, param);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> action)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        try
        {
            var result = await action(connection, transaction);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}