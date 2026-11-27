using System.Data;
using System.Data.Common;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Transactional unit of work over one Dapper connection.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    public UnitOfWork(IDbConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connection = connectionFactory.CreateConnection();
    }

    /// <inheritdoc />
    public DbConnection Connection => _connection;

    /// <inheritdoc />
    public DbTransaction? Transaction => _transaction;

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <summary>
    /// Executes a command inside the current transaction.
    /// </summary>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The number of affected rows.</returns>
    public Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
        => _connection.ExecuteAsync(new CommandDefinition(sql, parameters, _transaction, cancellationToken: cancellationToken));

    /// <summary>
    /// Executes a query inside the current transaction and returns the first row or a default value.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The first row, or <c>default</c>.</returns>
    public Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
        => _connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(sql, parameters, _transaction, cancellationToken: cancellationToken));

    /// <summary>
    /// Executes a query inside the current transaction and materializes every row.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The rows returned by the query.</returns>
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await _connection.QueryAsync<T>(
            new CommandDefinition(sql, parameters, _transaction, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _transaction?.Dispose();
        _connection.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await _connection.DisposeAsync();
        _disposed = true;
    }
}

/// <summary>
/// Creates <see cref="UnitOfWork"/> instances from the configured connection factory.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class UnitOfWorkFactory(IDbConnectionFactory connectionFactory) : IUnitOfWorkFactory
{
    /// <inheritdoc />
    public IUnitOfWork Create() => new UnitOfWork(connectionFactory);
}
