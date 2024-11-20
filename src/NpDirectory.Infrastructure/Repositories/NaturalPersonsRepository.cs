using Microsoft.EntityFrameworkCore;
using NpDirectory.Application.Common;
using NpDirectory.Application.ReadModels;
using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.Repositories;

public class NaturalPersonsRepository : Repository<NaturalPerson>, INaturalPersonsRepository
{
    private readonly AppDbContext _context;
    
    public NaturalPersonsRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<GetNaturalPersonInfoReadModel> GetNaturalPersonInfoAsync(int id)
{
    return _context.NaturalPersons
        .Where(np => np.Id == id)
        .Select(np => new GetNaturalPersonInfoReadModel
        {
            PersonInfo = new PersonInfoModel
            {
                Id = np.Id,
                FirstName = np.FirstName,
                LastName = np.LastName,
                Sex = np.Sex,
                PersonalNumber = np.PersonalNumber,
                BirthDate = np.BirthDate,
                City = np.City,
                ImageUrl = np.ImageUrl,
                PhoneNumbers = np.PhoneNumbers.Select(pn => new PhoneNumberModel
                {
                    Number = pn.Number,
                    Type = pn.Type
                }).ToList()
            },
            RelatedPersons = (
                from relation in _context.Relations
                join relatedPerson in _context.NaturalPersons 
                    on (relation.NaturalPersonId == id ? relation.RelatedPersonId : relation.NaturalPersonId) 
                    equals relatedPerson.Id
                where relation.NaturalPersonId == id || relation.RelatedPersonId == id
                select new RelatedPersonModel()
                {
                    Person = new PersonInfoModel
                    {
                        Id = relatedPerson.Id,
                        FirstName = relatedPerson.FirstName,
                        LastName = relatedPerson.LastName,
                        Sex = relatedPerson.Sex,
                        PersonalNumber = relatedPerson.PersonalNumber,
                        BirthDate = relatedPerson.BirthDate,
                        City = np.City,
                        ImageUrl = relatedPerson.ImageUrl,
                        PhoneNumbers = relatedPerson.PhoneNumbers.Select(pn => new PhoneNumberModel
                        {
                            Number = pn.Number,
                            Type = pn.Type
                        }).ToList()
                    },
                    RelationType = relation.Type
                }).ToList()
        })
        .FirstOrDefaultAsync();
}
}