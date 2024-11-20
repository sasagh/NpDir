using Microsoft.Extensions.Localization;
using NpDirectory.Application.Exceptions;
using NpDirectory.Application.Repositories;
using NpDirectory.Application.Requests;
using NpDirectory.Domain.Models;

namespace NpDirectory.Application.Services;

public class RelationService : IRelationService
{
    private readonly IUnitOfWork _uof;
    private readonly IStringLocalizer _localizer;

    public RelationService(IUnitOfWork uof, IStringLocalizer localizer)
    {
        _uof = uof;
        _localizer = localizer;
    }

    public async Task CreateRelationAsync(CreateRelationRequest request)
    {
        var firstPerson = await _uof.NaturalPersonsRepository.GetOneByFilterAsync(np => np.Id == request.FromId);
        if(firstPerson == null)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"] + $" id: {request.FromId}");
        
        var secondPerson = await _uof.NaturalPersonsRepository.GetOneByFilterAsync(np => np.Id == request.ToId);
        if(secondPerson == null)
            throw new NotFoundException(_localizer["Error.NaturalPerson.NotFound"] + $" id: {request.ToId}");
        
        var relationExists = await _uof.RelationsesRepository.GetOneByFilterAsync(r =>
            r.NaturalPersonId == request.FromId && r.RelatedPersonId == request.ToId
            || r.NaturalPersonId == request.ToId && r.RelatedPersonId == request.FromId) != null;
        
        if(relationExists)
            throw new RelationExistsException(_localizer["Error.Relation.AlreadyExists"]);

        var relation = new Relation()
        {
            NaturalPersonId = request.FromId,
            RelatedPersonId = request.ToId,
            Type = request.RelationType,
        };
        
        await _uof.RelationsesRepository.CreateAsync(relation);
        await _uof.SaveChangesAsync();
    }

    public async Task DeleteRelationAsync(int id)
    {
        var deleted = await _uof.RelationsesRepository.DeleteSingleAsync(id);
        if(!deleted)
            throw new NotFoundException(_localizer["Error.Relation.NotFound"]);
        
        await _uof.SaveChangesAsync();
    }

    public async Task DeleteRelationAsync(int fromId, int toId)
    {
        var relation = await _uof.RelationsesRepository.GetOneByFilterAsync(r =>
            r.NaturalPersonId == fromId && r.RelatedPersonId == toId
            || r.NaturalPersonId == toId && r.RelatedPersonId == fromId);
        
        if(relation == null)
            throw new NotFoundException(_localizer["Error.Relation.NotFound"]);
        
        var deleted = await _uof.RelationsesRepository.DeleteSingleAsync(relation.Id);
        if(!deleted)
            throw new NotFoundException(_localizer["Error.Relation.NotFound"]);
        
        await _uof.SaveChangesAsync();
    }
}