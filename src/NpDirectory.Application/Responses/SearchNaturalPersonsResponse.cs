using NpDirectory.Application.Common;

namespace NpDirectory.Application.Responses;

public class SearchNaturalPersonsResponse
{
    public List<PersonInfoModel> Persons { get; set; }
}