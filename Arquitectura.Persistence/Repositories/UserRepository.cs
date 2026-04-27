using Dapper;
using Microsoft.Data.SqlClient;
using Arquitectura.Domain.Entities;
using Arquitectura.Domain.Models;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.Repositories;

/// <summary>
/// User Repository using Dapper + Stored Procedures
/// Uses IDbConnectionFactory injected via constructor - follows DI pattern
/// </summary>
public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetByEmail",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return user != null 
                ? Result<User>.Success(user) 
                : Result<User>.Failure(ServiceErrors.NotFound("User", email));
        }
        catch (Exception ex)
        {
            return Result<User>.Failure("DATABASE_ERROR", ex.Message);
        }
    }

    public async Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetById",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return user != null 
                ? Result<User>.Success(user) 
                : Result<User>.Failure(ServiceErrors.NotFound("User", id.ToString()));
        }
        catch (Exception ex)
        {
            return Result<User>.Failure("DATABASE_ERROR", ex.Message);
        }
    }

    public async Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@FirstName", user.FirstName);
            parameters.Add("@LastName", user.LastName);
            parameters.Add("@PhoneNumber", user.PhoneNumber);
            parameters.Add("@CreatedBy", user.CreatedBy);

            var id = await connection.ExecuteScalarAsync<int>(
                "sp_User_Create",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            user.Id = id;
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure("DATABASE_ERROR", ex.Message);
        }
    }

    public async Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", user.Id);
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@FirstName", user.FirstName);
            parameters.Add("@LastName", user.LastName);
            parameters.Add("@PhoneNumber", user.PhoneNumber);
            parameters.Add("@IsActive", user.IsActive);
            parameters.Add("@RefreshToken", user.RefreshToken);
            parameters.Add("@RefreshTokenExpiresAt", user.RefreshTokenExpiresAt);
            parameters.Add("@LastLoginAt", user.LastLoginAt);
            parameters.Add("@UpdatedBy", user.UpdatedBy);

            await connection.ExecuteAsync(
                "sp_User_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure("DATABASE_ERROR", ex.Message);
        }
    }

    public async Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var count = await connection.ExecuteScalarAsync<int>(
                "sp_User_Exists",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return Result<bool>.Success(count > 0);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("DATABASE_ERROR", ex.Message);
        }
    }
}