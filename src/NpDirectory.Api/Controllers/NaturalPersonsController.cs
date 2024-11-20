using Microsoft.AspNetCore.Mvc;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Responses;
using NpDirectory.Application.Services;

namespace NpDirectory.Api.Controllers;

public class NaturalPersonsController : ControllerBaseWrapper
{
    private readonly INaturalPersonsService _naturalPersonsService;

    public NaturalPersonsController(INaturalPersonsService naturalPersonsService, ILogger<NaturalPersonsController> logger)
        : base(logger)
    {
        _naturalPersonsService = naturalPersonsService;
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNaturalPersonInfoResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetNaturalPersonInfo([FromRoute] int id)
        => ExecuteAsync(() => _naturalPersonsService.GetNaturalPersonInfoAsync(id));
    
    [HttpGet("fast-search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FastSearchNaturalPersonsResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> FastSearchNaturalPersons([FromQuery] FastSearchNaturalPersonRequest request)
        => ExecuteAsync(() => _naturalPersonsService.FastSearchNaturalPersonAsync(request));
    
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchNaturalPersonsResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> SearchNaturalPersons([FromQuery] SearchNaturalPersonsRequest request)
        => ExecuteAsync(() => _naturalPersonsService.SearchNaturalPersonsAsync(request));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> CreateNaturalPerson([FromBody] CreateNaturalPersonRequest request)
        => ExecuteAsync(() => _naturalPersonsService.CreateNaturalPersonAsync(request));
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> UpdateNaturalPerson([FromRoute] int id, [FromBody] UpdateNaturalPersonRequest request)
        => ExecuteAsync(() => _naturalPersonsService.UpdateNaturalPersonAsync(id, request));
    
    [HttpPut("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> UpdateNaturalPersonImage([FromRoute] int id, [FromForm] UpdateNaturalPersonImageRequest request)
        => ExecuteAsync(() => _naturalPersonsService.UpdateNaturalPersonImageAsync(id, request));
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteNaturalPerson([FromRoute] int id)
        => ExecuteAsync(() => _naturalPersonsService.DeleteNaturalPersonAsync(id));
    
    [HttpGet("report")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateReportResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GenerateReport()
        => ExecuteAsync(() => _naturalPersonsService.GenerateReportAsync());
}