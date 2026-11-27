using Modules.Auth.Domain;
using ResulTus;

namespace Modules.Auth.Persistence;

/// <summary>
/// Persistence port for user accounts. Implemented by the module infrastructure over stored procedures.
/// </summary>
public interface IUserRepository
{
    /// <summary>Loads a user by e-mail.</summary>
    /// <param name="email">The e-mail to look up.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The user, or a not-found error.</returns>
    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Loads a user by identifier.</summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The user, or a not-found error.</returns>
    Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a user and assigns its identifier.</summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The created user.</returns>
    Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Updates a user.</summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The updated user.</returns>
    Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Checks whether an e-mail is already registered.</summary>
    /// <param name="email">The e-mail to check.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns><c>true</c> when the e-mail exists.</returns>
    Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
