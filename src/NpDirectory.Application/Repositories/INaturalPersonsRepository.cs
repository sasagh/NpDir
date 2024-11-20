using NpDirectory.Application.ReadModels;
using NpDirectory.Domain.Models;

namespace NpDirectory.Application.Repositories;

public interface INaturalPersonsRepository : IRepository<NaturalPerson>
{
    Task<GetNaturalPersonInfoReadModel> GetNaturalPersonInfoAsync(int id);
    
    Task<GenerateReportReadModel> GenerateReportAsync();
}