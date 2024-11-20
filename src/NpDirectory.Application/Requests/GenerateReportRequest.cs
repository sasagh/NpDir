namespace NpDirectory.Application.Requests;

public class GenerateReportRequest
{
    public int Page { get; set; } = 0;

    public int PageSize { get; set; } = 10;
}