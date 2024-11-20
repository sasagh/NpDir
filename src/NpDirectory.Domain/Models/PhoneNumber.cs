using NpDirectory.Domain.Enum;

namespace NpDirectory.Domain.Models;

public class PhoneNumber
{
    public string Number { get; set; }
    
    public PhoneNumberType Type { get; set; }
    
    public int NaturalPersonId { get; set; }
    
    public virtual NaturalPerson NaturalPerson { get; set; }
}