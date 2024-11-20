using NpDirectory.Application.Common;

namespace NpDirectory.Application.Responses;

public class GenerateReportResponse
{
    public List<ReportPerson> Persons { get; set; }
}