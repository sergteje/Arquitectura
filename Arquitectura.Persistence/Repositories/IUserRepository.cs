using Arquitectura.Domain.Entities;
using Arquitectura.Domain.Models;

namespace Arquitectura.Persistence.Repositories;

public interface IUserRepository
{
    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<User>> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default);
}