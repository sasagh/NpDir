namespace NpDirectory.Application.Common;

public class ReportPerson
{
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string PersonalNumber { get; set; }
    
    public List<ReportItem> ReportItems { get; set; }
}