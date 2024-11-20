using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Requests;

public class SearchNaturalPersonsRequest
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public Sex Sex { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public int CityId { get; set; }
    
    public string PhoneNumber { get; set; }

    public int Page { get; set; } = 0;
    
    public int PageSize { get; set; } = 10;
}