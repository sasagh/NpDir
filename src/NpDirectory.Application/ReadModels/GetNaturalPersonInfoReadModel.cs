using NpDirectory.Application.Common;

namespace NpDirectory.Application.ReadModels;

public class GetNaturalPersonInfoReadModel
{
    public PersonInfoModel PersonInfo { get; set; }
    
    public ICollection<RelatedPersonModel> RelatedPersons { get; set; }
}