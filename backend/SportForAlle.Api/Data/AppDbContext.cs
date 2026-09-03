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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration in Data/Configurations, so a new
        // entity only needs its configuration file adding - nothing to wire here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
