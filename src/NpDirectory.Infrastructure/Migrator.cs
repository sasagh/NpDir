using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NpDirectory.Infrastructure;

public class Migrator
{
    private readonly IServiceProvider _serviceProvider;

    public Migrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Seed()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Database.ExecuteSqlRaw(@"
            INSERT INTO Cities (Name) SELECT 'Tbilisi' WHERE NOT EXISTS (SELECT 1 FROM Cities WHERE Name = 'Tbilisi');
            INSERT INTO Cities (Name) SELECT 'Choporti' WHERE NOT EXISTS (SELECT 1 FROM Cities WHERE Name = 'Choporti');
        ");
    }

    public void Migrate()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Database.Migrate();
    }
}