using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.Repositories;

public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(AppDbContext context) : base(context)
    {
    }
}