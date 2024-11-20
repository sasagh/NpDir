using Microsoft.EntityFrameworkCore;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<NaturalPerson> NaturalPersons { get; set; }
    
    public DbSet<City> Cities { get; set; }
    
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    
    public DbSet<Relation> Relations { get; set; }
}