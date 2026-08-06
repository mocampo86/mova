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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovaDbContext).Assembly);
    }
}
