using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Authentication;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.GoogleSubjectId == googleSubjectId));
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
    }

    public Task<(IReadOnlyList<User> Items, int TotalItems)> GetUsersByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        string? search = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;

        var query = _users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName ?? string.Empty).ToLowerInvariant().Contains(term)
                || (u.Email ?? string.Empty).ToLowerInvariant().Contains(term)
                || (u.PhoneNumber ?? string.Empty).ToLowerInvariant().Contains(term));
        }

        var sortBy = sort?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var sortField = sortBy.Length > 0 ? sortBy[0] : "fullName";
        var sortDirection = sortBy.Length > 1 ? sortBy[1] : "asc";

        query = sortField.ToLowerInvariant() switch
        {
            "email" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.Email)
                : query.OrderByDescending(u => u.Email),
            "createdat" or "created" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt),
            _ => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.FullName)
                : query.OrderByDescending(u => u.FullName)
        };

        var totalItems = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<User> Items, int TotalItems)>((items, totalItems));
    }

    public Task<(IReadOnlyList<User> Items, int TotalItems)> SearchUsersAsync(
        string? search,
        int page,
        int pageSize,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;

        var query = _users.AsQueryable()
            .Where(u => u.Status == UserStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName ?? string.Empty).ToLowerInvariant().Contains(term)
                || (u.Email ?? string.Empty).ToLowerInvariant().Contains(term)
                || (u.PhoneNumber ?? string.Empty).ToLowerInvariant().Contains(term));
        }

        var sortBy = sort?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var sortField = sortBy.Length > 0 ? sortBy[0] : "fullName";
        var sortDirection = sortBy.Length > 1 ? sortBy[1] : "asc";

        query = sortField.ToLowerInvariant() switch
        {
            "email" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.Email)
                : query.OrderByDescending(u => u.Email),
            "createdat" or "created" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt),
            _ => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(u => u.FullName)
                : query.OrderByDescending(u => u.FullName)
        };

        var totalItems = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IReadOnlyList<User> Items, int TotalItems)>((items, totalItems));
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}
