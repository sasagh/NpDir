using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NpDirectory.Application.Exceptions;
using NpDirectory.Domain.Models;

namespace NpDirectory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ControllerBaseWrapper : ControllerBase
{
    private readonly ILogger<ControllerBaseWrapper> _logger;

    protected ControllerBaseWrapper(ILogger<ControllerBaseWrapper> logger)
    {
        _logger = logger;
    }

    protected async Task<IActionResult> ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    
    protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private IActionResult HandleException(Exception ex)
    {
        _logger.LogError(ex, ex.Message);
        
        return ex switch
        {
            ValidationException validationException => BadRequest(validationException.Errors),
            NotFoundException notFoundException => NotFound(notFoundException.Message),
            NaturalPersonExistsException naturalPersonExistsException => Conflict(naturalPersonExistsException.Message),
            PersonalNumberExistsException personalNumberExists => Conflict(personalNumberExists.Message),
            PhoneNumberExistsException phoneNumberExists => Conflict(phoneNumberExists.Message),
            _ => StatusCode(500)
        };
    }
}