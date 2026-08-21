using Mova.Domain.Entities;

namespace Mova.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalItems)> GetUsersByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        string? search = null,
        string? sort = null,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalItems)> SearchUsersAsync(
        string? search,
        int page,
        int pageSize,
        string? sort = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
