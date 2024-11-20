using Microsoft.AspNetCore.Http;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Responses;

namespace NpDirectory.Application.Services;

public interface INaturalPersonsService
{
    Task<GetNaturalPersonInfoResponse> GetNaturalPersonInfoAsync(int id);
    
    Task<SearchNaturalPersonsResponse> SearchNaturalPersonsAsync(SearchNaturalPersonsRequest request);
    
    Task<FastSearchNaturalPersonsResponse> FastSearchNaturalPersonAsync(FastSearchNaturalPersonRequest request);
    
    Task<GenerateReportResponse> GenerateReportAsync();
    
    Task CreateNaturalPersonAsync(CreateNaturalPersonRequest request);
    
    Task UpdateNaturalPersonAsync(int id, UpdateNaturalPersonRequest request);
    
    Task UpdateNaturalPersonImageAsync(int id, UpdateNaturalPersonImageRequest request);
    
    Task DeleteNaturalPersonAsync(int id);
}