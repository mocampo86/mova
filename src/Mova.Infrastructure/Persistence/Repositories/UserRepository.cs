using Microsoft.EntityFrameworkCore;
using Mova.Application.Abstractions.Persistence;
using Mova.Domain.Entities;
using Mova.Infrastructure.Data;

namespace Mova.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private const int MaxPageSize = 100;
    private readonly MovaDbContext _context;

    public UserRepository(MovaDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .FirstOrDefaultAsync(u => u.GoogleSubjectId == googleSubjectId, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalItems)> GetUsersByComplexIdAsync(
        Guid sportsComplexId,
        int page,
        int pageSize,
        string? search = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        var reservationUserIds = _context.Reservations
            .Where(r => r.SportsComplexId == sportsComplexId)
            .Select(r => r.UserId)
            .Distinct();

        IQueryable<User> query = _context.Users
            .Where(u => reservationUserIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName ?? string.Empty).ToLower().Contains(term)
                || (u.Email ?? string.Empty).ToLower().Contains(term)
                || (u.PhoneNumber ?? string.Empty).ToLower().Contains(term));
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

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalItems)> SearchUsersAsync(
        string? search,
        int page,
        int pageSize,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize;
        pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

        IQueryable<User> query = _context.Users
            .Where(u => u.Status == Domain.Enums.UserStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.FullName ?? string.Empty).ToLower().Contains(term)
                || (u.Email ?? string.Empty).ToLower().Contains(term)
                || (u.PhoneNumber ?? string.Empty).ToLower().Contains(term));
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

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }
}
