using NpDirectory.Application.Repositories;

namespace NpDirectory.Application;

public interface IUnitOfWork
{
    INaturalPersonsRepository NaturalPersonsRepository { get; }
    
    ICityRepository CityRepository { get; }
    
    IPhoneNumbersRepository PhoneNumbersRepository { get; }
    
    IRelationsRepository RelationsesRepository { get; }
    
    Task SaveChangesAsync();
}