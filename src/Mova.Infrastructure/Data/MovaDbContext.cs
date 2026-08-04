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

    public DbSet<ComplexAdministrator> ComplexAdministrators => Set<ComplexAdministrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovaDbContext).Assembly);
    }
}
