using NpDirectory.Application.Common;

namespace NpDirectory.Application.Responses;

public class FastSearchNaturalPersonsResponse
{
    public List<PersonInfoModel> Persons { get; set; }
}