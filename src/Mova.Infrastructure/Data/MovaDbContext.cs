using Microsoft.EntityFrameworkCore;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data;

public sealed class MovaDbContext : DbContext
{
    public MovaDbContext(DbContextOptions<MovaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<SportsComplex> SportsComplexes => Set<SportsComplex>();

    public DbSet<ComplexAdministrator> ComplexAdministrators => Set<ComplexAdministrator>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<CourtSport> CourtSports => Set<CourtSport>();
    public DbSet<CourtAvailabilityRule> CourtAvailabilityRules => Set<CourtAvailabilityRule>();
    public DbSet<BusinessHours> BusinessHours => Set<BusinessHours>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<RecurringReservation> RecurringReservations => Set<RecurringReservation>();
    public DbSet<CourtBlock> CourtBlocks => Set<CourtBlock>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
    public DbSet<CancellationPolicy> CancellationPolicies => Set<CancellationPolicy>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovaDbContext).Assembly);
    }
}
