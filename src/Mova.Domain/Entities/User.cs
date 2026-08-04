using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string GoogleSubjectId { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public bool PhoneVerified { get; private set; }
    public UserStatus Status { get; private set; }
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Role> _roles = [];

    private User()
    {
    }

    public static User CreateFromGoogle(Guid id, string googleSubjectId, string email, string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleSubjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        var user = new User
        {
            Id = id,
            GoogleSubjectId = googleSubjectId,
            Email = email,
            FullName = fullName,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        user.AddRole(Role.User);
        return user;
    }

    public void AddRole(Role role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(Role role)
    {
        _roles.Remove(role);
    }

    public bool HasRole(Role role)
    {
        return _roles.Contains(role);
    }

    public void UpdateProfile(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        if (FullName == fullName)
        {
            return;
        }

        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteProfile(string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        PhoneNumber = phoneNumber;
        PhoneVerified = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        if (Status == UserStatus.Blocked)
        {
            return;
        }

        Status = UserStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }
}
