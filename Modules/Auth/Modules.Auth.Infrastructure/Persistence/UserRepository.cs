using System.Data;
using System.Data.Common;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using Modules.Auth.Domain;
using Modules.Auth.Persistence;
using ResulTus;
using ResulTus.Extensions;

using BuildingBlocks.Abstractions.Errors;
using Modules.Auth.Errors;

namespace Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// Dapper implementation of <see cref="IUserRepository"/> over the <c>sp_User_*</c> stored procedures.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    /// <inheritdoc />
    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var loaded = await ExecuteAsync(
            (connection, token) => connection.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(
                    "sp_User_GetByEmail",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token)),
            cancellationToken);

        return loaded.Bind(user => user is null
            ? Result<User>.Failure(AuthErrors.UserNotFoundByEmail(email))
            : Result<User>.Success(user));
    }

    /// <inheritdoc />
    public async Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var loaded = await ExecuteAsync(
            (connection, token) => connection.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(
                    "sp_User_GetById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token)),
            cancellationToken);

        return loaded.Bind(user => user is null
            ? Result<User>.Failure(AuthErrors.UserNotFoundById(id))
            : Result<User>.Success(user));
    }

    /// <inheritdoc />
    public Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            async (connection, token) =>
            {
                var parameters = new
                {
                    user.Email,
                    user.PasswordHash,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.CreatedBy
                };

                user.Id = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        "sp_User_Create",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: token));

                return user;
            },
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            user.Id,
            user.Email,
            user.PasswordHash,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsActive,
            user.RefreshToken,
            user.RefreshTokenExpiresAt,
            user.LastLoginAt,
            user.UpdatedBy
        };

        return ExecuteAsync(
            async (connection, token) =>
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "sp_User_Update",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: token));

                return user;
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var count = await ExecuteAsync(
            (connection, token) => connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "sp_User_Exists",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token)),
            cancellationToken);

        return count.Map(value => value > 0);
    }

    private async Task<Result<T>> ExecuteAsync<T>(
        Func<DbConnection, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var result = await Result.TryAsync(
            async token =>
            {
                await using var connection = connectionFactory.CreateConnection();
                return await operation(connection, token);
            },
            cancellationToken);

        return result.MapError(error => CommonErrors.Database(error.Message ?? "Unknown database error"));
    }
}
