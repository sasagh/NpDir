using Microsoft.EntityFrameworkCore;
using NpDirectory.Application.Common;
using NpDirectory.Application.ReadModels;
using NpDirectory.Application.Repositories;
using NpDirectory.Domain.Enum;
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
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<GenerateReportReadModel> GenerateReportAsync()
    {
        var relationCounts = await _context.Relations
            .AsNoTracking()
            .GroupBy(x => new { PersonId = x.NaturalPersonId, x.Type })
            .Select(g => new
            {
                PersonId = g.Key.PersonId,
                RelationType = g.Key.Type,
                Count = g.Count()
            })
            .Union(
                _context.Relations
                    .AsNoTracking()
                    .GroupBy(x => new { PersonId = x.RelatedPersonId, x.Type })
                    .Select(g => new
                    {
                        PersonId = g.Key.PersonId,
                        RelationType = g.Key.Type,
                        Count = g.Count()
                    })
            )
            .GroupBy(x => new { x.PersonId, x.RelationType })
            .Select(g => new
            {
                PersonId = g.Key.PersonId,
                Type = g.Key.RelationType,
                RelationCounts = g.Sum(x => x.Count)
            })
            .ToListAsync();
        
        var reportPersons = await _context.NaturalPersons
            .AsNoTracking()
            .Select(np => new ReportPerson
            {
                Id = np.Id,
                FirstName = np.FirstName,
                LastName = np.LastName,
                PersonalNumber = np.PersonalNumber,
                ReportItems = new List<ReportItem>()
            })
            .ToListAsync();

        foreach (var reportPerson in reportPersons)
        {
            foreach (var relationType in Enum.GetValues<RelationType>())
            {
                var relationCount = relationCounts.FirstOrDefault(x => x.PersonId == reportPerson.Id && x.Type == relationType);
                reportPerson.ReportItems.Add(new ReportItem
                {
                    Type = relationType,
                    Count = relationCount?.RelationCounts ?? 0
                });
            }
        }

        return new GenerateReportReadModel
        {
            Persons = reportPersons
        };
    }

}