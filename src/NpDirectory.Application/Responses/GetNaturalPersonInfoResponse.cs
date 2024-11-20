using NpDirectory.Application.Common;

namespace NpDirectory.Application.Responses;

public class GetNaturalPersonInfoResponse
{
    public PersonInfoModel PersonInfo { get; set; }
    
    public ICollection<RelatedPersonModel> RelatedPersons { get; set; }
}

