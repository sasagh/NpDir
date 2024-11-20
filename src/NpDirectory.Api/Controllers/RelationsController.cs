using Microsoft.AspNetCore.Mvc;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Services;

namespace NpDirectory.Api.Controllers;

public class RelationsController : ControllerBaseWrapper
{
    private readonly IRelationService _relationService;

    public RelationsController(IRelationService relationService, ILogger<RelationsController> logger)
        : base(logger)
    {
        _relationService = relationService;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> CreateRelation([FromBody] CreateRelationRequest request)
        => ExecuteAsync(() => _relationService.CreateRelationAsync(request));
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteRelation([FromRoute] int id)
        => ExecuteAsync(() => _relationService.DeleteRelationAsync(id));
    
    [HttpDelete("{idFrom:int}/{idTo:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteRelation([FromRoute] int idFrom, [FromRoute] int idTo)
        => ExecuteAsync(() => _relationService.DeleteRelationAsync(idFrom, idTo));
}