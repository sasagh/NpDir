using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Common;

public class ReportItem
{
    public RelationType Type { get; set; }
    
    public int Count { get; set; }
}