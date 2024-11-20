using Microsoft.EntityFrameworkCore;
using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.Repositories;

public class PhoneNumbersRepository : Repository<PhoneNumber>, IPhoneNumbersRepository
{
    private AppDbContext _context;
    public PhoneNumbersRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> CheckIfPhoneNumberExistsAsync(IEnumerable<string> phoneNumbers)
    {
        return _context.PhoneNumbers.AsNoTracking().AnyAsync(p => phoneNumbers.Contains(p.Number));
    }
}