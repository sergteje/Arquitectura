using System.Data.Common;

namespace BuildingBlocks.Abstractions.Persistence;

/// <summary>
/// Creates database connections for one unit of work. The returned connection is not opened.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new, closed connection to the application database.
    /// </summary>
    /// <returns>A closed <see cref="DbConnection"/> that the caller owns and disposes.</returns>
    DbConnection CreateConnection();
}

/// <summary>
/// Transactional unit of work over a single connection.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying connection.
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the active transaction, or <c>null</c> when none was started.
    /// </summary>
    DbTransaction? Transaction { get; }

    /// <summary>
    /// Opens the connection when needed and starts a transaction.
    /// </summary>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active transaction.
    /// </summary>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active transaction.
    /// </summary>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command inside the current transaction.
    /// </summary>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query inside the current transaction and returns the first row or a default value.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The first row, or <c>default</c>.</returns>
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query inside the current transaction and materializes every row.
    /// </summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="sql">The SQL text or stored procedure name.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The rows returned by the query.</returns>
    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Creates a <see cref="IUnitOfWork"/> scoped to the caller.
/// </summary>
public interface IUnitOfWorkFactory
{
    /// <summary>
    /// Creates a new unit of work.
    /// </summary>
    IUnitOfWork Create();
}

/// <summary>
/// Database connection configuration, read from <c>ConnectionStrings:DefaultConnection</c>.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the connection string used by every module.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
