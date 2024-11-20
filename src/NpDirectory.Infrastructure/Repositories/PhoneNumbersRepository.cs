using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.Repositories;

public class PhoneNumbersRepository : Repository<PhoneNumber>, IPhoneNumbersRepository
{
    public PhoneNumbersRepository(AppDbContext context) : base(context)
    {
    }
}