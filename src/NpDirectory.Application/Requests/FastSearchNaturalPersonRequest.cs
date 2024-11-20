namespace NpDirectory.Application.Requests;

public class FastSearchNaturalPersonRequest
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public int Page { get; set; } = 0;
    
    public int PageSize { get; set; } = 10;
}