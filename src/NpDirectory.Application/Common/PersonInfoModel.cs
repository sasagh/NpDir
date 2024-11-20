using NpDirectory.Domain.Enum;
using NpDirectory.Domain.Models;

namespace NpDirectory.Application.Common;

public class PersonInfoModel
{
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public Sex Sex { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public City City { get; set; }
    
    public ICollection<PhoneNumberModel> PhoneNumbers { get; set; }
    
    public string ImageUrl { get; set; }
}