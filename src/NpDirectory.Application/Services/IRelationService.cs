using NpDirectory.Application.Requests;

namespace NpDirectory.Application.Services;

public interface IRelationService
{
    Task CreateRelationAsync(CreateRelationRequest request);
    
    Task DeleteRelationAsync(int id);
    
    Task DeleteRelationAsync(int fromId, int toId);
}