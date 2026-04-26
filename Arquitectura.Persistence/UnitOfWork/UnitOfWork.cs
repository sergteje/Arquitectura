using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work pattern for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    
    void BeginTransaction();
    void Commit();
    void Rollback();
    
    Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = default, CancellationToken ct = default);
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = default, CancellationToken ct = default);
}

/// <summary>
/// Unit of Work implementation with Dapper
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;
    
    public UnitOfWork(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public IDbConnection Connection => _connection;
    
    public IDbTransaction? Transaction => _transaction;
    
    public void BeginTransaction()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        
        _transaction = _connection.BeginTransaction();
    }
    
    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }
    
    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }
    
    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        return await _connection.ExecuteAsync(sql, param, _transaction);
    }
    
    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        return await _connection.QuerySingleOrDefaultAsync<T>(sql, param, _transaction);
    }
    
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        var result = await _connection.QueryAsync<T>(sql, param, _transaction);
        return result.ToList();
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _connection.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Scoped unit of work factory
/// </summary>
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}

public class UnitOfWorkFactory(IDbConnectionFactory connectionFactory) : IUnitOfWorkFactory
{
    public IUnitOfWork Create() => new UnitOfWork(connectionFactory.CreateConnection());
}