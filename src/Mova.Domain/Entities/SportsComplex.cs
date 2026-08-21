using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class SportsComplex
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public ComplexStatus Status { get; private set; }
    public bool AllowUserRecurringReservations { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<ComplexAdministrator> ComplexAdministrators { get; private set; } = [];

    private SportsComplex()
    {
    }

    public static SportsComplex Create(
        string name,
        string description,
        string address,
        string city,
        decimal? latitude,
        decimal? longitude,
        string phoneNumber,
        string email,
        ComplexStatus status = ComplexStatus.Active)
    {
        ValidateFields(name, description, address, city, latitude, longitude, phoneNumber, email);

        return new SportsComplex
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            Address = address.Trim(),
            City = city.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            PhoneNumber = phoneNumber.Trim(),
            Email = email.Trim(),
            Status = status,
            AllowUserRecurringReservations = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string description,
        string address,
        string city,
        decimal? latitude,
        decimal? longitude,
        string phoneNumber,
        string email)
    {
        ValidateFields(name, description, address, city, latitude, longitude, phoneNumber, email);

        Name = name.Trim();
        Description = description.Trim();
        Address = address.Trim();
        City = city.Trim();
        Latitude = latitude;
        Longitude = longitude;
        PhoneNumber = phoneNumber.Trim();
        Email = email.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == ComplexStatus.Active)
        {
            return;
        }

        Status = ComplexStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == ComplexStatus.Inactive)
        {
            return;
        }

        Status = ComplexStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRecurringReservationSettings(bool allowUserRecurringReservations)
    {
        AllowUserRecurringReservations = allowUserRecurringReservations;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateFields(
        string name,
        string description,
        string address,
        string city,
        decimal? latitude,
        decimal? longitude,
        string phoneNumber,
        string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (name.Length > 255)
        {
            throw new ArgumentException("Name must not exceed 255 characters.", nameof(name));
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException("Description must not exceed 2000 characters.", nameof(description));
        }

        if (address.Length > 255)
        {
            throw new ArgumentException("Address must not exceed 255 characters.", nameof(address));
        }

        if (city.Length > 255)
        {
            throw new ArgumentException("City must not exceed 255 characters.", nameof(city));
        }

        if (latitude is < -90m or > 90m)
        {
            throw new ArgumentException("Latitude must be between -90 and 90.", nameof(latitude));
        }

        if (longitude is < -180m or > 180m)
        {
            throw new ArgumentException("Longitude must be between -180 and 180.", nameof(longitude));
        }

        if (phoneNumber.Length > 50)
        {
            throw new ArgumentException("Phone number must not exceed 50 characters.", nameof(phoneNumber));
        }

        if (email.Length > 255)
        {
            throw new ArgumentException("Email must not exceed 255 characters.", nameof(email));
        }
    }
}
