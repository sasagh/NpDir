using NpDirectory.Domain.Models;

namespace NpDirectory.Application.Repositories;

public interface IPhoneNumbersRepository : IRepository<PhoneNumber>
{
    Task<bool> CheckIfPhoneNumberExistsAsync(IEnumerable<string> phoneNumbers);
}