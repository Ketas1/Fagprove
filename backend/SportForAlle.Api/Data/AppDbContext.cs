using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data;

/// <summary>
/// The application's Entity Framework context. Services depend on this directly -
/// there is no repository layer, see docs/adr/0012-lagdelt-monolitt.md.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();

    public DbSet<Staff> Staff => Set<Staff>();

    public DbSet<Guardian> Guardians => Set<Guardian>();

    public DbSet<Borrower> Borrowers => Set<Borrower>();

    public DbSet<Equipment> Equipment => Set<Equipment>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<ContactAttempt> ContactAttempts => Set<ContactAttempt>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Ban> Bans => Set<Ban>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration in Data/Configurations, so a new
        // entity only needs its configuration file adding - nothing to wire here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
