using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.Repositories;

public class RelationsRepository : Repository<Relation>, IRelationsRepository
{
    public RelationsRepository(AppDbContext context) : base(context)
    {
    }
}