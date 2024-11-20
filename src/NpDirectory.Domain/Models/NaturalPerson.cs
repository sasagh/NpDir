using NpDirectory.Domain.Enum;

namespace NpDirectory.Domain.Models;

public class NaturalPerson
{
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public Sex Sex { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public int CityId { get; set; }
    
    public virtual City City { get; set; }
    
    public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
    
    public string ImageUrl { get; set; }
}