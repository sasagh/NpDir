using NpDirectory.Application;
using NpDirectory.Application.Repositories;

namespace NpDirectory.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        INaturalPersonsRepository naturalPersonsRepository,
        ICityRepository cityRepository,
        IPhoneNumbersRepository phoneNumbersRepository,
        IRelationsRepository relationsesRepository)
    {
        _context = context;
        NaturalPersonsRepository = naturalPersonsRepository;
        CityRepository = cityRepository;
        PhoneNumbersRepository = phoneNumbersRepository;
        RelationsesRepository = relationsesRepository;
    }
    
    public INaturalPersonsRepository NaturalPersonsRepository { get; }
    public ICityRepository CityRepository { get; }
    public IPhoneNumbersRepository PhoneNumbersRepository { get; }
    public IRelationsRepository RelationsesRepository { get; }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}