using NpDirectory.Domain.Enum;

namespace NpDirectory.Domain.Models;

public class Relation
{
    public int Id { get; set; }
    
    public int NaturalPersonId { get; set; }
    
    public virtual NaturalPerson NaturalPerson { get; set; }
    
    public int RelatedPersonId { get; set; }
    
    public virtual NaturalPerson RelatedPerson { get; set; }
    
    public RelationType Type { get; set; }
}