using System.Net;
using Microsoft.FeatureManagement;

namespace NpDirectory.Api;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IFeatureManager _featureManager;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IFeatureManager featureManager)
    {
        _next = next;
        _logger = logger;
        _featureManager = featureManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. Message: {Message}", ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Message = "An unexpected error occurred. Please try again later.",
                Detailed = await _featureManager.IsEnabledAsync("EnableErrorMessageDetails") ? ex.Message : null
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}