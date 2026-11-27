using System.Data;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using Modules.Auth.Domain;
using Modules.Auth.Persistence;
using ResulTus;

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
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(
                    "sp_User_GetByEmail",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return user is not null
                ? Result<User>.Success(user)
                : Result<User>.Failure(AuthErrors.UserNotFoundByEmail(email));
        }
        catch (Exception exception)
        {
            return Result<User>.Failure(CommonErrors.Database(exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(
                    "sp_User_GetById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return user is not null
                ? Result<User>.Success(user)
                : Result<User>.Failure(AuthErrors.UserNotFoundById(id));
        }
        catch (Exception exception)
        {
            return Result<User>.Failure(CommonErrors.Database(exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var parameters = new
            {
                user.Email,
                user.PasswordHash,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.CreatedBy
            };

            var id = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "sp_User_Create",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            user.Id = id;
            return Result<User>.Success(user);
        }
        catch (Exception exception)
        {
            return Result<User>.Failure(CommonErrors.Database(exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

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

            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_User_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return Result<User>.Success(user);
        }
        catch (Exception exception)
        {
            return Result<User>.Failure(CommonErrors.Database(exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "sp_User_Exists",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return Result<bool>.Success(count > 0);
        }
        catch (Exception exception)
        {
            return Result<bool>.Failure(CommonErrors.Database(exception.Message));
        }
    }
}
