using Dapper;
using Microsoft.Data.SqlClient;
using Arquitectura.Domain.Entities;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.Repositories;

/// <summary>
/// User Repository using Dapper + Stored Procedures
/// Uses IDbConnectionFactory injected via constructor - follows DI pattern
/// </summary>
public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);

        return await connection.QueryFirstOrDefaultAsync<User>(
            "sp_User_GetByEmail",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        return await connection.QueryFirstOrDefaultAsync<User>(
            "sp_User_GetById",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Email", user.Email);
        parameters.Add("@PasswordHash", user.PasswordHash);
        parameters.Add("@FirstName", user.FirstName);
        parameters.Add("@LastName", user.LastName);
        parameters.Add("@PhoneNumber", user.PhoneNumber);
        parameters.Add("@CreatedBy", user.CreatedBy);

        return await connection.ExecuteScalarAsync<int>(
            "sp_User_Create",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
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
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);

        var count = await connection.ExecuteScalarAsync<int>(
            "sp_User_Exists",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return count > 0;
    }
}